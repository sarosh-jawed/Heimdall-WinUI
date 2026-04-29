using System.Globalization;
using System.Text.RegularExpressions;
using Heimdall.Application.Configuration;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Infrastructure.Export;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Export;

public sealed class HtmlExportServiceTests
{
    [Fact]
    public async Task ExportAsync_SelectedCategoriesCreateSeparateHtmlFiles()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            HtmlExportService service = CreateService();

            HtmlExportResult result = await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            string[] files = Directory.GetFiles(outputFolder)
                .Select(Path.GetFileName)
                .Where(fileName => fileName is not null)
                .Cast<string>()
                .ToArray();

            Assert.Contains(files, file => Regex.IsMatch(file, @"^ArtNewBooks\d{4}-\d{2}-\d{2}\.html$"));
            Assert.Contains(files, file => Regex.IsMatch(file, @"^BusinessNewBooks\d{4}-\d{2}-\d{2}\.html$"));

            Assert.Contains(result.GeneratedFiles, file => file.Value.StartsWith("ArtNewBooks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(result.GeneratedFiles, file => file.Value.StartsWith("BusinessNewBooks", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_FileNamesMatchExpectedDateFormat()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            HtmlExportService service = CreateService();

            HtmlExportResult result = await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            Assert.All(result.GeneratedFiles, file =>
            {
                Assert.Matches(@"^(ArtNewBooks|BusinessNewBooks|CannotSortBooks|RunSummary)\d{4}-\d{2}-\d{2}\.(html|txt)$", file.Value);
            });
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_SavesFilesDirectlyIntoSelectedOutputFolder()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            HtmlExportService service = CreateService();

            await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            string[] subfolders = Directory.GetDirectories(outputFolder);
            string[] files = Directory.GetFiles(outputFolder);

            Assert.Empty(subfolders);
            Assert.NotEmpty(files);
            Assert.All(files, file => Assert.Equal(outputFolder, Path.GetDirectoryName(file)));
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_GeneratesCannotSortBooksWhenUncategorizedRecordsExist()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            HtmlExportService service = CreateService();

            await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            string cannotSortFile = Directory.GetFiles(outputFolder)
                .Single(file => Regex.IsMatch(Path.GetFileName(file), @"^CannotSortBooks\d{4}-\d{2}-\d{2}\.html$"));

            string html = await File.ReadAllTextAsync(cannotSortFile);

            Assert.Contains("Cannot Sort Books", html);
            Assert.Contains("Unmatched Book", html);
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_GeneratesRunSummaryFile()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            HtmlExportService service = CreateService();

            await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            string runSummaryFile = Directory.GetFiles(outputFolder)
                .Single(file => Regex.IsMatch(Path.GetFileName(file), @"^RunSummary\d{4}-\d{2}-\d{2}\.txt$"));

            string text = await File.ReadAllTextAsync(runSummaryFile);

            Assert.Contains("Heimdall Run Summary", text);
            Assert.Contains("Generated Files:", text);
            Assert.Contains("Cannot Sort Count: 1", text);
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_OverwritesExpectedExistingFiles()
    {
        string outputFolder = CreateTempFolder();

        try
        {
            string dateToken = DateTimeOffset.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string artFilePath = Path.Combine(outputFolder, $"ArtNewBooks{dateToken}.html");

            await File.WriteAllTextAsync(artFilePath, "OLD CONTENT");

            HtmlExportService service = CreateService();

            await service.ExportAsync(
                CreatePreviewResult(),
                outputFolder);

            string artHtml = await File.ReadAllTextAsync(artFilePath);

            Assert.DoesNotContain("OLD CONTENT", artHtml);
            Assert.Contains("New Art Books", artHtml);
            Assert.Contains("Art Book", artHtml);
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    private static HtmlExportService CreateService()
    {
        return new HtmlExportService(
            new HeimdallConfig(),
            new HtmlEmailRenderer(),
            new RunSummaryService());
    }

    private static EmailPreviewResult CreatePreviewResult()
    {
        EmailCategoryPreview artPreview = new(
            CreateCategory("Art"),
            new[]
            {
                new EmailBookItem(
                    "art-001",
                    "Art Book",
                    "Artist, Alex",
                    "A summary for the art email.")
            });

        EmailCategoryPreview businessPreview = new(
            CreateCategory("Business"),
            new[]
            {
                new EmailBookItem(
                    "business-001",
                    "Business Book",
                    "Manager, Morgan",
                    "A summary for the business email.")
            });

        EmailBookItem cannotSortBook = new(
            "cannot-sort-001",
            "Unmatched Book",
            "Unknown Author",
            "This book did not match a selected category.");

        return new EmailPreviewResult(
            new[]
            {
                artPreview,
                businessPreview
            },
            new[]
            {
                cannotSortBook
            },
            Array.Empty<string>());
    }

    private static CategoryDefinition CreateCategory(string displayName)
    {
        return new CategoryDefinition(
            displayName,
            $"{displayName.Replace(" ", string.Empty)}Subjects.txt");
    }

    private static string CreateTempFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"heimdall-export-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
