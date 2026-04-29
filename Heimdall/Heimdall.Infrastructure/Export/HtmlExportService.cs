using System.Globalization;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Infrastructure.Export;

public sealed class HtmlExportService : IHtmlExportService
{
    private readonly HeimdallConfig _config;
    private readonly IHtmlEmailRenderer _htmlEmailRenderer;
    private readonly IRunSummaryService _runSummaryService;

    public HtmlExportService(
        HeimdallConfig config,
        IHtmlEmailRenderer htmlEmailRenderer,
        IRunSummaryService runSummaryService)
    {
        _config = config;
        _htmlEmailRenderer = htmlEmailRenderer;
        _runSummaryService = runSummaryService;
    }

    public async Task<HtmlExportResult> ExportAsync(
        EmailPreviewResult previewResult,
        string outputFolder,
        ExportRunContext? runContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previewResult);

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            throw new ArgumentException("Output folder cannot be blank.", nameof(outputFolder));
        }

        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(outputFolder);

        var generatedFiles = new List<OutputFileName>();
        var runStartedAt = DateTimeOffset.Now;
        var dateToken = runStartedAt.ToString(_config.Output.DateFormat, CultureInfo.InvariantCulture);

        foreach (var categoryPreview in previewResult.Categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = BuildCategoryFileName(categoryPreview.Category.DisplayName, dateToken);
            var filePath = Path.Combine(outputFolder, fileName.Value);
            var html = _htmlEmailRenderer.RenderCategoryHtml(categoryPreview);

            await File.WriteAllTextAsync(filePath, html, cancellationToken);
            generatedFiles.Add(fileName);
        }

        if (previewResult.CannotSortBooks.Count > 0)
        {
            var cannotSortFileName = BuildFileName(
                _config.Output.CannotSortFileNameTemplate,
                category: null,
                dateToken);

            var cannotSortPath = Path.Combine(outputFolder, cannotSortFileName.Value);
            var cannotSortHtml = _htmlEmailRenderer.RenderCannotSortHtml(previewResult.CannotSortBooks);

            await File.WriteAllTextAsync(cannotSortPath, cannotSortHtml, cancellationToken);
            generatedFiles.Add(cannotSortFileName);
        }

        var runSummaryFileName = BuildFileName(
            _config.Output.RunSummaryFileNameTemplate,
            category: null,
            dateToken);

        // Add RunSummary before rendering it so the summary file lists itself too.
        generatedFiles.Add(runSummaryFileName);

        var runEndedAt = DateTimeOffset.Now;

        var runSummary = new RunSummary
        {
            RunStartedAt = runStartedAt,
            RunEndedAt = runEndedAt,
            SourceCsvPath = runContext?.SourceCsvPath ?? string.Empty,
            SubjectListMode = runContext?.SubjectListMode ?? "WorkflowExport",
            SubjectListFolderPath = runContext?.SubjectListFolderPath,
            TotalRecordsRead = runContext?.TotalRecordsRead ?? CountUniqueRecords(previewResult),
            SelectedCategories = runContext?.SelectedCategories.Count > 0
                ? runContext.SelectedCategories
                : previewResult.Categories.Select(category => category.Category.Key).ToArray(),
            MatchedRecordCounts = previewResult.Categories.ToDictionary(
                category => category.Category.Key,
                category => category.ActiveBookCount),
            RemovedRecordCounts = runContext?.RemovedRecordCounts.Count > 0
                ? runContext.RemovedRecordCounts
                : previewResult.Categories.ToDictionary(
                    category => category.Category.Key,
                    category => category.Books.Count - category.ActiveBookCount),
            CannotSortCount = previewResult.CannotSortBooks.Count,
            GeneratedFiles = generatedFiles.ToArray(),
            Warnings = previewResult.Warnings
        };

        var runSummaryPath = Path.Combine(outputFolder, runSummaryFileName.Value);
        await File.WriteAllTextAsync(runSummaryPath, _runSummaryService.RenderText(runSummary), cancellationToken);

        return new HtmlExportResult(
            outputFolder,
            generatedFiles,
            runSummary,
            previewResult.Warnings);
    }

    private OutputFileName BuildCategoryFileName(string category, string dateToken)
    {
        return BuildFileName(_config.Output.CategoryFileNameTemplate, category, dateToken);
    }

    private static OutputFileName BuildFileName(
        string template,
        string? category,
        string dateToken)
    {
        var fileName = template.Replace("{{Date}}", dateToken, StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(category))
        {
            fileName = fileName.Replace(
                "{{Category}}",
                SanitizeCategoryName(category),
                StringComparison.OrdinalIgnoreCase);
        }

        return new OutputFileName(fileName);
    }

    private static string SanitizeCategoryName(string category)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();

        var cleaned = new string(category
            .Where(character => !invalidCharacters.Contains(character))
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray());

        return string.IsNullOrWhiteSpace(cleaned)
            ? "Category"
            : cleaned;
    }

    private static int CountUniqueRecords(EmailPreviewResult previewResult)
    {
        var bookIds = previewResult.Categories
            .SelectMany(category => category.Books.Select(book => book.BookId))
            .Concat(previewResult.CannotSortBooks.Select(book => book.BookId))
            .Where(bookId => !string.IsNullOrWhiteSpace(bookId))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return bookIds.Count();
    }
}
