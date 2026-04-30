using System.Text;
using Heimdall.Application.Configuration;
using Heimdall.Application.Errors;
using Heimdall.Application.Workflow;
using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Extraction;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;
using Heimdall.Infrastructure.Bragi;
using Heimdall.Infrastructure.Csv;
using Heimdall.Infrastructure.Export;
using Heimdall.Infrastructure.Html;
using Heimdall.Infrastructure.Matching;
using Microsoft.Extensions.Logging;
using Heimdall.Domain.Models;

namespace Heimdall.Tests.Regression;

public sealed class AutomatedRegressionTests
{
    private static readonly string[] MissingRequiredColumnFixture = ["instances.title"];

    [Fact]
    public async Task OfficialCsvRegression_LoadsThirtyFourRecordsAndValidatesRequiredSchema()
    {
        string csvPath = WriteTempCsv(BuildOfficialCsv(rowCount: 34));

        try
        {
            HeimdallConfig config = new();
            CsvSchemaValidator schemaValidator = new(config);
            CsvBookRecordReader reader = CreateCsvReader(config);

            schemaValidator.ValidateOrThrow(HeaderLine.Split(','));

            CsvLoadResult result = await reader.ReadAsync(csvPath);

            Assert.Equal(34, result.TotalRows);
            Assert.Equal(34, result.Books.Count);
            Assert.Contains(result.Books, book => book.Title == "Bridge <Safety> Book");
            Assert.Contains(result.Books, book => book.RawSubjects.Contains("History", StringComparison.OrdinalIgnoreCase));

            UserFriendlyException exception = Assert.Throws<UserFriendlyException>(() =>
                schemaValidator.ValidateOrThrow(MissingRequiredColumnFixture));

            Assert.Equal(HeimdallErrorCode.CsvMissingRequiredColumns, exception.ErrorCode);
            Assert.Contains("missing required columns", exception.Title, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            TryDeleteFile(csvPath);
        }
    }

    [Fact]
    public async Task FreshBragiRegression_GeneratesSubjectListsBuildsPreviewAndExportsFiles()
    {
        string csvPath = WriteTempCsv(BuildOfficialCsv(rowCount: 6));
        string bragiOutputFolder = CreateTempFolder("heimdall-regression-bragi");
        string exportFolder = CreateTempFolder("heimdall-regression-export");

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);

            SubjectListLoadResult subjectListResult =
                await orchestrator.GenerateFreshSubjectListsAsync(csvPath, bragiOutputFolder);

            Assert.NotEmpty(subjectListResult.CategorySubjectLists);
            Assert.True(File.Exists(Path.Combine(bragiOutputFolder, "RunSummary.txt")));
            Assert.True(File.Exists(Path.Combine(bragiOutputFolder, "NotCategorizedSubjects.txt")));

            CategoryKey[] selectedCategories = [.. subjectListResult.CategorySubjectLists
                .Select(list => list.Category.Key)
                .Where(key =>
                    key.Value.Equals("History", StringComparison.OrdinalIgnoreCase) ||
                    key.Value.Equals("Education", StringComparison.OrdinalIgnoreCase) ||
                    key.Value.Equals("Fiction", StringComparison.OrdinalIgnoreCase))
                .Distinct()];

            Assert.NotEmpty(selectedCategories);

            EmailPreviewResult preview = await orchestrator.BuildPreviewAsync(selectedCategories);

            Assert.NotEmpty(preview.Categories);
            Assert.True(
                preview.Categories.Sum(category => category.ActiveBookCount) > 0 ||
                preview.CannotSortBooks.Count > 0);

            HtmlExportResult exportResult = await orchestrator.ExportAsync(exportFolder);

            Assert.NotEmpty(exportResult.GeneratedFiles);
            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("RunSummary", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(Directory.GetFiles(exportFolder), file =>
                Path.GetFileName(file).StartsWith("RunSummary", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            TryDeleteFile(csvPath);
            TryDeleteDirectory(bragiOutputFolder);
            TryDeleteDirectory(exportFolder);
        }
    }

    [Fact]
    public async Task ExistingFolderRegression_CoversMatchingRemovalHtmlCannotSortAndRunSummary()
    {
        string csvPath = WriteTempCsv(BuildRegressionCsv());
        string subjectFolder = CreateExistingSubjectListFolder();
        string exportFolder = CreateTempFolder("heimdall-regression-existing-export");

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            EmailPreviewResult preview = await orchestrator.BuildPreviewAsync(
                [
                    new CategoryKey("History"),
                    new CategoryKey("Education"),
                    new CategoryKey("Fiction")
                ]);

            Assert.Equal(3, preview.Categories.Count);
            Assert.Single(preview.CannotSortBooks);

            Assert.Contains(
                GetCategory(preview, "History").ActiveBooks,
                book => book.BookId == "multi-001");

            Assert.Contains(
                GetCategory(preview, "Education").ActiveBooks,
                book => book.BookId == "multi-001");

            Assert.Contains(
                GetCategory(preview, "Fiction").ActiveBooks,
                book => book.BookId == "fiction-001");

            EmailPreviewResult previewAfterRemoval = await orchestrator.RemoveBookFromPreviewAsync(
                new CategoryKey("Education"),
                "multi-001");

            Assert.DoesNotContain(
                GetCategory(previewAfterRemoval, "Education").ActiveBooks,
                book => book.BookId == "multi-001");

            Assert.Contains(
                GetCategory(previewAfterRemoval, "History").ActiveBooks,
                book => book.BookId == "multi-001");

            HtmlExportResult exportResult = await orchestrator.ExportAsync(exportFolder);

            string historyHtml = await File.ReadAllTextAsync(
                FindGeneratedFilePath(exportResult, "HistoryNewBooks"));

            string educationHtml = await File.ReadAllTextAsync(
                FindGeneratedFilePath(exportResult, "EducationNewBooks"));

            string cannotSortHtml = await File.ReadAllTextAsync(
                FindGeneratedFilePath(exportResult, "CannotSortBooks"));

            string runSummaryText = await File.ReadAllTextAsync(
                FindGeneratedFilePath(exportResult, "RunSummary"));

            Assert.Contains("Bridge &lt;Safety&gt; Book", historyHtml);
            Assert.Contains("Encoded &lt;summary&gt; &amp; safe.", historyHtml);

            Assert.DoesNotContain("Bridge &lt;Safety&gt; Book", educationHtml);
            Assert.Contains("Cannot Sort Books", cannotSortHtml);
            Assert.Contains("Cannot Sort Book", cannotSortHtml);

            Assert.Contains("Heimdall Run Summary", runSummaryText);
            Assert.Contains("Input:", runSummaryText);
            Assert.Contains("Source CSV Path:", runSummaryText);
            Assert.Contains("Subject List Mode:", runSummaryText);
            Assert.Contains("Selected Categories:", runSummaryText);
            Assert.Contains("Matched Records by Category:", runSummaryText);
            Assert.Contains("Removed Records by Category:", runSummaryText);
            Assert.Contains("Cannot Sort Count:", runSummaryText);
            Assert.Contains("Generated Files:", runSummaryText);

            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("HistoryNewBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("EducationNewBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("FictionNewBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("CannotSortBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(exportResult.GeneratedFiles, file =>
                file.Value.StartsWith("RunSummary", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            TryDeleteFile(csvPath);
            TryDeleteDirectory(subjectFolder);
            TryDeleteDirectory(exportFolder);
        }
    }

    private static WorkflowOrchestrator CreateOrchestrator()
    {
        HeimdallConfig heimdallConfig = new();
        CsvSchemaValidator schemaValidator = new(heimdallConfig);
        SummaryExtractor summaryExtractor = new();
        CsvBookRecordReader csvReader = new(heimdallConfig, schemaValidator, summaryExtractor);

        BragiCoreOptions bragiOptions = new();
        CategoryFileDetector categoryFileDetector = new(bragiOptions);
        SubjectListFolderReader subjectListFolderReader = new(categoryFileDetector);

        BragiSubjectListGenerator bragiGenerator = new(
            csvReader,
            bragiOptions,
            new SubjectExtractionService(),
            new CategorizationService(),
            new TextExportService());

        HtmlEmailRenderer htmlRenderer = new();
        RunSummaryService runSummaryService = new();
        HtmlExportService htmlExportService = new(heimdallConfig, htmlRenderer, runSummaryService);

        return new WorkflowOrchestrator(
            csvReader,
            bragiGenerator,
            subjectListFolderReader,
            new BookCategoryMatcher(),
            new EmailPreviewBuilder(),
            htmlExportService,
            new WizardSessionStore(),
            new RegressionLogger<WorkflowOrchestrator>());
    }

    private static CsvBookRecordReader CreateCsvReader(HeimdallConfig config)
    {
        CsvSchemaValidator schemaValidator = new(config);
        SummaryExtractor summaryExtractor = new();

        return new CsvBookRecordReader(config, schemaValidator, summaryExtractor);
    }

    private static string CreateExistingSubjectListFolder()
    {
        string folder = CreateTempFolder("heimdall-regression-existing-subjects");

        File.WriteAllText(Path.Combine(folder, "HistorySubjects.txt"), "History\r\n", Encoding.UTF8);
        File.WriteAllText(Path.Combine(folder, "EducationSubjects.txt"), "Education\r\n", Encoding.UTF8);
        File.WriteAllText(Path.Combine(folder, "FictionSubjects.txt"), "Fiction\r\n", Encoding.UTF8);

        return folder;
    }

    private static string BuildRegressionCsv()
    {
        return string.Join(
            Environment.NewLine,
            HeaderLine,
            CsvLine(
                "multi-001",
                "Bridge <Safety> Book",
                "Author & One",
                SummaryNote("Encoded <summary> & safe."),
                "History; Education",
                "hrid-001",
                "barcode-001",
                "QA 001",
                "Library Display",
                "Book",
                "2026"),
            CsvLine(
                "fiction-001",
                "Fiction Only Book",
                "Author Two",
                SummaryNote("Fiction summary."),
                "Fiction",
                "hrid-002",
                "barcode-002",
                "PS 002",
                "Library Display",
                "Book",
                "2026"),
            CsvLine(
                "cannot-001",
                "Cannot Sort Book",
                "Author Three",
                SummaryNote("CannotSort summary."),
                "Unmapped Subject",
                "hrid-003",
                "barcode-003",
                "XX 003",
                "Library Display",
                "Book",
                "2026"));
    }

    private static string BuildOfficialCsv(int rowCount)
    {
        StringBuilder builder = new();
        builder.AppendLine(HeaderLine);

        builder.AppendLine(CsvLine(
            "multi-001",
            "Bridge <Safety> Book",
            "Author & One",
            SummaryNote("Encoded <summary> & safe."),
            "History; Education",
            "hrid-001",
            "barcode-001",
            "QA 001",
            "Library Display",
            "Book",
            "2026"));

        builder.AppendLine(CsvLine(
            "fiction-001",
            "Fiction Only Book",
            "Author Two",
            SummaryNote("Fiction summary."),
            "Fiction",
            "hrid-002",
            "barcode-002",
            "PS 002",
            "Library Display",
            "Book",
            "2026"));

        builder.AppendLine(CsvLine(
            "cannot-001",
            "Cannot Sort Book",
            "Author Three",
            SummaryNote("CannotSort summary."),
            "Unmapped Subject",
            "hrid-003",
            "barcode-003",
            "XX 003",
            "Library Display",
            "Book",
            "2026"));

        for (int index = 4; index <= rowCount; index++)
        {
            builder.AppendLine(CsvLine(
                $"generated-{index:000}",
                $"Generated History Book {index}",
                $"Generated Author {index}",
                SummaryNote($"Generated summary {index}."),
                "History",
                $"hrid-{index:000}",
                $"barcode-{index:000}",
                $"D {index:000}",
                "Library Display",
                "Book",
                "2026"));
        }

        return builder.ToString();
    }

    private const string HeaderLine =
        "instances.id,instances.title,instances.instance_primary_contributor,instances.notes,instances.subjects,instances.hrid,items.barcode,items.effective_call_number,effective_location.name,mtypes.name,instances.publication";

    private static string SummaryNote(string summary)
    {
        return $"[{{\"note\":\"Summary: {summary}\",\"staffOnly\":false}}]";
    }

    private static string CsvLine(params string[] values)
    {
        return string.Join(",", values.Select(Escape));
    }

    private static string Escape(string value)
    {
        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value}\"";
        }

        return value;
    }

    private static string WriteTempCsv(string content)
    {
        string path = Path.Combine(Path.GetTempPath(), $"heimdall-regression-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    private static string CreateTempFolder(string prefix)
    {
        string folder = Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static EmailCategoryPreview GetCategory(
        EmailPreviewResult preview,
        string categoryDisplayName)
    {
        return preview.Categories.Single(category =>
            category.Category.DisplayName.Equals(categoryDisplayName, StringComparison.OrdinalIgnoreCase));
    }

    private static string FindGeneratedFilePath(HtmlExportResult result, string fileNamePrefix)
    {
        OutputFileName fileName = result.GeneratedFiles.Single(file =>
            file.Value.StartsWith(fileNamePrefix, StringComparison.OrdinalIgnoreCase));

        return Path.Combine(result.OutputFolder, fileName.Value);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Test cleanup should never hide the real assertion failure.
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Test cleanup should never hide the real assertion failure.
        }
    }

    private sealed class RegressionLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // Regression tests only need a real ILogger implementation so workflow logging
            // remains exercised without writing test logs to disk.
        }
    }
}
