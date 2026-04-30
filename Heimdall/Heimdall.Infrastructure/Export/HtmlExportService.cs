using System.Globalization;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;
using Heimdall.Application.Errors;

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
            throw new UserFriendlyException(
                HeimdallErrorCode.OutputFolderBlank,
                "Output folder required",
                "No output folder was selected.",
                "Go back to Load Input and select an output folder.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        EnsureOutputFolderExists(outputFolder);
        await EnsureOutputFolderWritableAsync(outputFolder, cancellationToken);

        var generatedFiles = new List<OutputFileName>();
        var warnings = new List<string>(previewResult.Warnings);
        var runStartedAt = DateTimeOffset.Now;
        var dateToken = runStartedAt.ToString(_config.Output.DateFormat, CultureInfo.InvariantCulture);

        foreach (var categoryPreview in previewResult.Categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = BuildCategoryFileName(categoryPreview.Category.DisplayName, dateToken);
            var filePath = Path.Combine(outputFolder, fileName.Value);
            var html = _htmlEmailRenderer.RenderCategoryHtml(categoryPreview);

            AddOverwriteWarningIfNeeded(filePath, warnings);
            await WriteTextFileSafelyAsync(filePath, html, cancellationToken);

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

            AddOverwriteWarningIfNeeded(cannotSortPath, warnings);
            await WriteTextFileSafelyAsync(cannotSortPath, cannotSortHtml, cancellationToken);

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
            Warnings = warnings
        };

        var runSummaryPath = Path.Combine(outputFolder, runSummaryFileName.Value);
        AddOverwriteWarningIfNeeded(runSummaryPath, warnings);
        await WriteTextFileSafelyAsync(runSummaryPath, _runSummaryService.RenderText(runSummary), cancellationToken);

        return new HtmlExportResult(
            outputFolder,
            generatedFiles,
            runSummary,
            warnings);
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

    private static void EnsureOutputFolderExists(string outputFolder)
    {
        try
        {
            Directory.CreateDirectory(outputFolder);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.OutputFolderNotWritable,
                "Output folder cannot be created",
                "Heimdall does not have permission to create or access the selected output folder.",
                "Choose a normal folder such as Documents or Desktop, then try again.",
                ex);
        }
        catch (IOException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.OutputFolderNotWritable,
                "Output folder unavailable",
                "Heimdall could not create or access the selected output folder.",
                "Choose a different output folder and try again.",
                ex);
        }
    }

    private static async Task EnsureOutputFolderWritableAsync(
        string outputFolder,
        CancellationToken cancellationToken)
    {
        string testFilePath = Path.Combine(
            outputFolder,
            $".heimdall-write-test-{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(testFilePath, "Heimdall write test", cancellationToken);
            File.Delete(testFilePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.OutputFolderNotWritable,
                "Output folder is not writable",
                "Heimdall does not have permission to write files into the selected output folder.",
                "Select a different folder or update the folder permissions.",
                ex);
        }
        catch (IOException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.OutputFolderNotWritable,
                "Output folder is not writable",
                "Heimdall could not write a test file into the selected output folder.",
                "Close any locked files or select a different output folder.",
                ex);
        }
        finally
        {
            TryDeleteFile(testFilePath);
        }
    }

    private static async Task WriteTextFileSafelyAsync(
        string filePath,
        string contents,
        CancellationToken cancellationToken)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, contents, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.ExportFailed,
                "Export file cannot be written",
                $"Heimdall does not have permission to write {Path.GetFileName(filePath)}.",
                "Close the file if it is open or choose a different output folder.",
                ex);
        }
        catch (IOException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.ExportFailed,
                "Export file unavailable",
                $"Heimdall could not write {Path.GetFileName(filePath)}.",
                "Close the file if it is open in another program, then try again.",
                ex);
        }
    }

    private static void AddOverwriteWarningIfNeeded(
        string filePath,
        ICollection<string> warnings)
    {
        if (File.Exists(filePath))
        {
            warnings.Add($"Existing export file was overwritten: {Path.GetFileName(filePath)}");
        }
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Best-effort cleanup only. Export validation should not fail because the probe file
            // was already handled by the main write check.
        }
    }
}
