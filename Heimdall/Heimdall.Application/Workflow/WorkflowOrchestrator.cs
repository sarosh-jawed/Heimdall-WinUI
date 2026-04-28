using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Heimdall.Application.Workflow;

public sealed class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly ICsvBookRecordReader _csvBookRecordReader;
    private readonly IBragiSubjectListGenerator _bragiSubjectListGenerator;
    private readonly ISubjectListFolderReader _subjectListFolderReader;
    private readonly IBookCategoryMatcher _bookCategoryMatcher;
    private readonly IEmailPreviewBuilder _emailPreviewBuilder;
    private readonly IHtmlExportService _htmlExportService;
    private readonly WizardSessionStore _sessionStore;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        ICsvBookRecordReader csvBookRecordReader,
        IBragiSubjectListGenerator bragiSubjectListGenerator,
        ISubjectListFolderReader subjectListFolderReader,
        IBookCategoryMatcher bookCategoryMatcher,
        IEmailPreviewBuilder emailPreviewBuilder,
        IHtmlExportService htmlExportService,
        WizardSessionStore sessionStore,
        ILogger<WorkflowOrchestrator> logger)
    {
        _csvBookRecordReader = csvBookRecordReader;
        _bragiSubjectListGenerator = bragiSubjectListGenerator;
        _subjectListFolderReader = subjectListFolderReader;
        _bookCategoryMatcher = bookCategoryMatcher;
        _emailPreviewBuilder = emailPreviewBuilder;
        _htmlExportService = htmlExportService;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public Task<CsvLoadResult> LoadCsvAsync(
        string csvPath,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Load CSV",
            async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _csvBookRecordReader.ReadAsync(csvPath, cancellationToken);

                _sessionStore.Reset();
                _sessionStore.SourceCsvPath = csvPath;
                _sessionStore.CsvLoadResult = result;

                _logger.LogInformation(
                    "CSV loaded. Path={CsvPath} TotalRows={TotalRows} BookCount={BookCount}",
                    csvPath,
                    result.TotalRows,
                    result.Books.Count);

                return result;
            });
    }

    public Task<SubjectListLoadResult> GenerateFreshSubjectListsAsync(
        string csvPath,
        string workingOutputFolder,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Generate fresh Bragi subject lists",
            async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _bragiSubjectListGenerator.GenerateAsync(
                    csvPath,
                    workingOutputFolder,
                    cancellationToken);

                if (!result.Success || result.SubjectListLoadResult is null)
                {
                    throw new InvalidOperationException("Fresh Bragi subject-list generation did not produce usable subject lists.");
                }

                _sessionStore.SubjectListMode = "GenerateFresh";
                _sessionStore.SubjectListFolderPath = workingOutputFolder;
                _sessionStore.SubjectListLoadResult = result.SubjectListLoadResult;
                _sessionStore.ResetPreviewState();

                _logger.LogInformation(
                    "Fresh Bragi subject lists generated. OutputFolder={OutputFolder} CategoryCount={CategoryCount}",
                    workingOutputFolder,
                    result.SubjectListLoadResult.CategorySubjectLists.Count);

                return result.SubjectListLoadResult;
            });
    }

    public Task<SubjectListLoadResult> LoadExistingSubjectListsAsync(
        string subjectListFolder,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Load existing Bragi subject lists",
            async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _subjectListFolderReader.ReadAsync(
                    subjectListFolder,
                    cancellationToken);

                _sessionStore.SubjectListMode = "ExistingFolder";
                _sessionStore.SubjectListFolderPath = subjectListFolder;
                _sessionStore.SubjectListLoadResult = result;
                _sessionStore.ResetPreviewState();

                _logger.LogInformation(
                    "Existing Bragi subject lists loaded. Folder={SubjectListFolder} CategoryCount={CategoryCount}",
                    subjectListFolder,
                    result.CategorySubjectLists.Count);

                return result;
            });
    }

    public Task<EmailPreviewResult> BuildPreviewAsync(
        IReadOnlyList<CategoryKey> selectedCategories,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Build email preview",
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_sessionStore.CsvLoadResult is null)
                {
                    throw new InvalidOperationException("CSV records must be loaded before building a preview.");
                }

                if (_sessionStore.SubjectListLoadResult is null)
                {
                    throw new InvalidOperationException("Subject lists must be loaded before building a preview.");
                }

                if (selectedCategories.Count == 0)
                {
                    throw new InvalidOperationException("At least one broad category must be selected before building a preview.");
                }

                var matchResult = _bookCategoryMatcher.Match(
                    _sessionStore.CsvLoadResult.Books,
                    _sessionStore.SubjectListLoadResult.CategorySubjectLists,
                    selectedCategories);

                var previewResult = _emailPreviewBuilder.Build(matchResult);
                var previewWithRemovals = ApplyRemovedBookSelections(previewResult);

                _sessionStore.CategoryMatchResult = matchResult;
                _sessionStore.PreviewResult = previewWithRemovals;

                _logger.LogInformation(
                    "Preview built. SelectedCategoryCount={SelectedCategoryCount} PreviewCategoryCount={PreviewCategoryCount} CannotSortCount={CannotSortCount}",
                    selectedCategories.Count,
                    previewWithRemovals.Categories.Count,
                    previewWithRemovals.CannotSortBooks.Count);

                return Task.FromResult(previewWithRemovals);
            });
    }

    public Task<EmailPreviewResult> RemoveBookFromPreviewAsync(
        CategoryKey category,
        string bookId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Remove book from preview",
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_sessionStore.PreviewResult is null)
                {
                    throw new InvalidOperationException("A preview must be built before a book can be removed.");
                }

                if (string.IsNullOrWhiteSpace(bookId))
                {
                    throw new ArgumentException("Book ID cannot be blank.", nameof(bookId));
                }

                var alreadyRemoved = _sessionStore.RemovedBookSelections.Any(selection =>
                    selection.CategoryKey.Value.Equals(category.Value, StringComparison.OrdinalIgnoreCase)
                    && selection.BookId.Equals(bookId, StringComparison.OrdinalIgnoreCase));

                if (!alreadyRemoved)
                {
                    _sessionStore.RemovedBookSelections.Add(new RemovedBookSelection(category, bookId));
                }

                var updatedPreview = ApplyRemovedBookSelections(_sessionStore.PreviewResult);
                _sessionStore.PreviewResult = updatedPreview;

                _logger.LogInformation(
                    "Book removed from category preview. Category={Category} BookId={BookId}",
                    category.Value,
                    bookId);

                return Task.FromResult(updatedPreview);
            });
    }

    public Task<HtmlExportResult> ExportAsync(
        string outputFolder,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLoggingAsync(
            "Export HTML files",
            async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_sessionStore.PreviewResult is null)
                {
                    throw new InvalidOperationException("A preview must be built before export.");
                }

                var result = await _htmlExportService.ExportAsync(
                    _sessionStore.PreviewResult,
                    outputFolder,
                    cancellationToken);

                _sessionStore.OutputFolderPath = outputFolder;

                _logger.LogInformation(
                    "HTML export completed. OutputFolder={OutputFolder} GeneratedFileCount={GeneratedFileCount}",
                    outputFolder,
                    result.GeneratedFiles.Count);

                return result;
            });
    }

    private EmailPreviewResult ApplyRemovedBookSelections(EmailPreviewResult previewResult)
    {
        var updatedCategories = previewResult.Categories
            .Select(categoryPreview =>
            {
                var removedBookIds = _sessionStore.RemovedBookSelections
                    .Where(selection => selection.CategoryKey.Value.Equals(
                        categoryPreview.Category.Key.Value,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(selection => selection.BookId)
                    .ToArray();

                return new EmailCategoryPreview(
                    categoryPreview.Category,
                    categoryPreview.Books,
                    removedBookIds);
            })
            .ToArray();

        return new EmailPreviewResult(
            updatedCategories,
            previewResult.CannotSortBooks,
            previewResult.Warnings);
    }

    private async Task<T> ExecuteWithLoggingAsync<T>(
        string operationName,
        Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Workflow operation was cancelled. Operation={Operation}", operationName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow operation failed. Operation={Operation}", operationName);
            throw;
        }
    }
}
