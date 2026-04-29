using System.Text;
using Heimdall.Application.Configuration;
using Heimdall.Application.Workflow;
using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Extraction;
using Heimdall.Domain.ValueObjects;
using Heimdall.Infrastructure.Bragi;
using Heimdall.Infrastructure.Csv;
using Heimdall.Infrastructure.Export;
using Heimdall.Infrastructure.Html;
using Heimdall.Infrastructure.Matching;
using Microsoft.Extensions.Logging.Abstractions;

namespace Heimdall.Tests.Workflow;

public sealed class PreviewRemovalExportTests
{
    [Fact]
    public async Task BuildPreviewAsync_ShowsMatchedBooksGroupedByCategory()
    {
        string csvPath = WriteTempCsv();
        string subjectFolder = CreateSubjectListFolder();

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[]
                {
                    new CategoryKey("Art"),
                    new CategoryKey("History")
                });

            Assert.Contains(preview.Categories, category => category.Category.DisplayName == "Art");
            Assert.Contains(preview.Categories, category => category.Category.DisplayName == "History");

            Assert.Contains(
                preview.Categories.Single(category => category.Category.DisplayName == "Art").Books,
                book => book.Title == "Shared Art History Book");

            Assert.Contains(
                preview.Categories.Single(category => category.Category.DisplayName == "History").Books,
                book => book.Title == "Shared Art History Book");
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
        }
    }

    [Fact]
    public async Task RemoveBookFromPreviewAsync_RemovesOnlyFromSelectedCategory()
    {
        string csvPath = WriteTempCsv();
        string subjectFolder = CreateSubjectListFolder();

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[]
                {
                    new CategoryKey("Art"),
                    new CategoryKey("History")
                });

            string bookId = preview.Categories
                .Single(category => category.Category.DisplayName == "Art")
                .Books
                .Single(book => book.Title == "Shared Art History Book")
                .BookId;

            var updatedPreview = await orchestrator.RemoveBookFromPreviewAsync(
                new CategoryKey("Art"),
                bookId);

            Assert.DoesNotContain(
                updatedPreview.Categories.Single(category => category.Category.DisplayName == "Art").ActiveBooks,
                book => book.Title == "Shared Art History Book");

            Assert.Contains(
                updatedPreview.Categories.Single(category => category.Category.DisplayName == "History").ActiveBooks,
                book => book.Title == "Shared Art History Book");
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_DoesNotWriteRemovedBooksToThatCategoryHtml()
    {
        string csvPath = WriteTempCsv();
        string subjectFolder = CreateSubjectListFolder();
        string outputFolder = CreateTempFolder();

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[]
                {
                    new CategoryKey("Art"),
                    new CategoryKey("History")
                });

            string bookId = preview.Categories
                .Single(category => category.Category.DisplayName == "Art")
                .Books
                .Single(book => book.Title == "Shared Art History Book")
                .BookId;

            await orchestrator.RemoveBookFromPreviewAsync(new CategoryKey("Art"), bookId);

            await orchestrator.ExportAsync(outputFolder);

            string artHtmlPath = Directory.GetFiles(outputFolder)
                .Single(file => Path.GetFileName(file).StartsWith("ArtNewBooks", StringComparison.OrdinalIgnoreCase));

            string historyHtmlPath = Directory.GetFiles(outputFolder)
                .Single(file => Path.GetFileName(file).StartsWith("HistoryNewBooks", StringComparison.OrdinalIgnoreCase));

            string artHtml = await File.ReadAllTextAsync(artHtmlPath);
            string historyHtml = await File.ReadAllTextAsync(historyHtmlPath);

            Assert.DoesNotContain("Shared Art History Book", artHtml);
            Assert.Contains("Shared Art History Book", historyHtml);
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task BuildPreviewAsync_IncludesCannotSortSummary()
    {
        string csvPath = WriteTempCsv();
        string subjectFolder = CreateSubjectListFolder();

        try
        {
            WorkflowOrchestrator orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[]
                {
                    new CategoryKey("Art"),
                    new CategoryKey("History")
                });

            Assert.Contains(preview.CannotSortBooks, book => book.Title == "Unknown Subject Book");
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
        }
    }


    private static WorkflowOrchestrator CreateOrchestrator()
    {
        HeimdallConfig config = new();
        CsvSchemaValidator schemaValidator = new(config);
        SummaryExtractor summaryExtractor = new();
        CsvBookRecordReader csvReader = new(config, schemaValidator, summaryExtractor);

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
        HtmlExportService htmlExportService = new(config, htmlRenderer, runSummaryService);

        return new WorkflowOrchestrator(
            csvReader,
            bragiGenerator,
            subjectListFolderReader,
            new BookCategoryMatcher(),
            new EmailPreviewBuilder(),
            htmlExportService,
            new WizardSessionStore(),
            NullLogger<WorkflowOrchestrator>.Instance);
    }

    private static string WriteTempCsv()
    {
        string content = string.Join(
            Environment.NewLine,
            "instances.id,instances.title,instances.instance_primary_contributor,instances.notes,instances.subjects",
            CsvLine("instance-001", "Shared Art History Book", "Doe, Jane", "", "Art; History"),
            CsvLine("instance-002", "Art Only Book", "Artist, Alex", "", "Art"),
            CsvLine("instance-003", "Unknown Subject Book", "Writer, Test", "", "Unmatched Subject"));

        string path = Path.Combine(Path.GetTempPath(), $"heimdall-preview-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    private static string CreateSubjectListFolder()
    {
        string folder = CreateTempFolder();

        File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Art\r\n");
        File.WriteAllText(Path.Combine(folder, "HistorySubjects.txt"), "History\r\n");

        return folder;
    }

    private static string CreateTempFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"heimdall-preview-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
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
}




