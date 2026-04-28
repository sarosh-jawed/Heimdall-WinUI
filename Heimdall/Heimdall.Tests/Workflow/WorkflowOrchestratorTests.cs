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
using Microsoft.Extensions.Logging;

namespace Heimdall.Tests.Workflow;

public sealed class WorkflowOrchestratorTests
{
    [Fact]
    public async Task LoadCsvAsync_LoadsOfficialCsvShape()
    {
        var csvPath = WriteTempCsv();
        var outputFolder = CreateTempFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            var result = await orchestrator.LoadCsvAsync(csvPath);

            Assert.Equal(3, result.TotalRows);
            Assert.Equal(3, result.Books.Count);
        }
        finally
        {
            Cleanup(csvPath, outputFolder);
        }
    }

    [Fact]
    public async Task GenerateFreshSubjectListsAsync_CreatesFiles()
    {
        var csvPath = WriteTempCsv();
        var outputFolder = CreateTempFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            var result = await orchestrator.GenerateFreshSubjectListsAsync(csvPath, outputFolder);

            Assert.NotEmpty(result.CategorySubjectLists);
            Assert.True(File.Exists(Path.Combine(outputFolder, "HistorySubjects.txt")));
            Assert.True(File.Exists(Path.Combine(outputFolder, "RunSummary.txt")));
        }
        finally
        {
            Cleanup(csvPath, outputFolder);
        }
    }

    [Fact]
    public async Task LoadExistingSubjectListsAsync_ReadsExistingFiles()
    {
        var folder = CreateExistingSubjectListFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            var result = await orchestrator.LoadExistingSubjectListsAsync(folder);

            Assert.Contains(result.CategorySubjectLists, list => list.Category.DisplayName == "Art");
            Assert.Contains(result.CategorySubjectLists, list => list.Category.DisplayName == "History");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task BuildPreviewAsync_ReturnsSelectedCategoriesOnly()
    {
        var csvPath = WriteTempCsv();
        var subjectFolder = CreateExistingSubjectListFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[] { new CategoryKey("History") });

            Assert.Single(preview.Categories);
            Assert.Equal("History", preview.Categories[0].Category.DisplayName);
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
        }
    }

    [Fact]
    public async Task RemoveBookFromPreviewAsync_UpdatesPreviewForOnlySelectedCategory()
    {
        var csvPath = WriteTempCsv();
        var subjectFolder = CreateExistingSubjectListFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);

            var preview = await orchestrator.BuildPreviewAsync(
                new[]
                {
                    new CategoryKey("History"),
                    new CategoryKey("Education")
                });

            var historyPreview = preview.Categories.Single(category => category.Category.DisplayName == "History");
            var bookId = historyPreview.ActiveBooks.Single().BookId;

            var updatedPreview = await orchestrator.RemoveBookFromPreviewAsync(
                new CategoryKey("History"),
                bookId);

            var updatedHistoryPreview = updatedPreview.Categories.Single(category => category.Category.DisplayName == "History");
            var updatedEducationPreview = updatedPreview.Categories.Single(category => category.Category.DisplayName == "Education");

            Assert.Empty(updatedHistoryPreview.ActiveBooks);
            Assert.Single(updatedEducationPreview.ActiveBooks);
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_WritesHtmlFiles()
    {
        var csvPath = WriteTempCsv();
        var subjectFolder = CreateExistingSubjectListFolder();
        var outputFolder = CreateTempFolder();

        try
        {
            var orchestrator = CreateOrchestrator();

            await orchestrator.LoadCsvAsync(csvPath);
            await orchestrator.LoadExistingSubjectListsAsync(subjectFolder);
            await orchestrator.BuildPreviewAsync(new[] { new CategoryKey("Art") });

            var result = await orchestrator.ExportAsync(outputFolder);

            Assert.Contains(result.GeneratedFiles, file => file.Value.StartsWith("ArtNewBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result.GeneratedFiles, file => file.Value.StartsWith("RunSummary", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(Directory.GetFiles(outputFolder), file => Path.GetFileName(file).StartsWith("ArtNewBooks", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(subjectFolder, recursive: true);
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task Workflow_RespectsCancellationToken()
    {
        var orchestrator = CreateOrchestrator();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            orchestrator.BuildPreviewAsync(
                new[] { new CategoryKey("Art") },
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Workflow_LogsErrors()
    {
        var logger = new ListLogger<WorkflowOrchestrator>();
        var orchestrator = CreateOrchestrator(logger);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            orchestrator.LoadCsvAsync("missing-file.csv"));

        Assert.Contains(logger.Entries, entry =>
            entry.Level == LogLevel.Error &&
            entry.Message.Contains("Workflow operation failed", StringComparison.OrdinalIgnoreCase));
    }

    private static WorkflowOrchestrator CreateOrchestrator(
        ILogger<WorkflowOrchestrator>? logger = null)
    {
        var config = new HeimdallConfig();
        var schemaValidator = new CsvSchemaValidator(config);
        var summaryExtractor = new SummaryExtractor();
        var csvReader = new CsvBookRecordReader(config, schemaValidator, summaryExtractor);

        var bragiOptions = new BragiCoreOptions();
        var categoryFileDetector = new CategoryFileDetector(bragiOptions);
        var subjectListFolderReader = new SubjectListFolderReader(categoryFileDetector);
        var bragiGenerator = new BragiSubjectListGenerator(
            csvReader,
            bragiOptions,
            new SubjectExtractionService(),
            new CategorizationService(),
            new TextExportService());

        var htmlRenderer = new HtmlEmailRenderer();
        var runSummaryService = new RunSummaryService();
        var htmlExportService = new HtmlExportService(config, htmlRenderer, runSummaryService);

        return new WorkflowOrchestrator(
            csvReader,
            bragiGenerator,
            subjectListFolderReader,
            new BookCategoryMatcher(),
            new EmailPreviewBuilder(),
            htmlExportService,
            new WizardSessionStore(),
            logger ?? new ListLogger<WorkflowOrchestrator>());
    }

    private static string WriteTempCsv()
    {
        var content = string.Join(
            Environment.NewLine,
            "instances.id,instances.title,instances.instance_primary_contributor,instances.notes,instances.subjects",
            CsvLine("instance-001", "Art Book", "Doe, Jane", "", "Art"),
            CsvLine("instance-002", "History Education Book", "Smith, John", "", "History; Education"),
            CsvLine("instance-003", "Unknown Book", "Writer, Test", "", "Unmatched Subject"));

        var path = Path.Combine(Path.GetTempPath(), $"heimdall-workflow-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    private static string CreateExistingSubjectListFolder()
    {
        var folder = CreateTempFolder();

        File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Art\r\n");
        File.WriteAllText(Path.Combine(folder, "HistorySubjects.txt"), "History\r\n");
        File.WriteAllText(Path.Combine(folder, "EducationSubjects.txt"), "Education\r\n");

        return folder;
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"heimdall-workflow-{Guid.NewGuid():N}");
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

    private static void Cleanup(string csvPath, string outputFolder)
    {
        if (File.Exists(csvPath))
        {
            File.Delete(csvPath);
        }

        if (Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(
                logLevel,
                formatter(state, exception),
                exception));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception);
}
