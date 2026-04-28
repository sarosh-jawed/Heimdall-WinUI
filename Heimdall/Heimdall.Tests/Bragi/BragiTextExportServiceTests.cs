using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Extraction;

namespace Heimdall.Tests.Bragi;

public sealed class BragiTextExportServiceTests
{
    [Fact]
    public async Task ExportAsync_GeneratesSortedDeduplicatedCategoryFilesAndSummaryFiles()
    {
        var outputFolder = CreateTempFolder();

        try
        {
            var options = new BragiCoreOptions
            {
                CategoryRules = new[]
                {
                    new CategoryRule
                    {
                        Key = "history",
                        DisplayName = "History",
                        OutputFileName = "HistorySubjects.txt",
                        IncludeKeywords = new[] { "history" },
                        SortOrder = 1,
                        Enabled = true
                    }
                }
            };

            var rule = options.CategoryRules.Single();

            var categorizationResult = new SubjectCategorizationResult(
                new[]
                {
                    new SubjectCategoryAssignment(
                        new ExtractedSubject("z history", "z history", null, null, 2),
                        rule,
                        "test"),
                    new SubjectCategoryAssignment(
                        new ExtractedSubject("a history", "a history", null, null, 3),
                        rule,
                        "test"),
                    new SubjectCategoryAssignment(
                        new ExtractedSubject("a history", "a history", null, null, 4),
                        rule,
                        "test")
                },
                new[] { "Unknown subject" },
                new Dictionary<string, int>
                {
                    ["history"] = 3
                });

            var service = new TextExportService();

            await service.ExportAsync(outputFolder, options.CategoryRules, categorizationResult, options);

            var categoryFile = Path.Combine(outputFolder, "HistorySubjects.txt");
            var notCategorizedFile = Path.Combine(outputFolder, "NotCategorizedSubjects.txt");
            var runSummaryFile = Path.Combine(outputFolder, "RunSummary.txt");

            Assert.True(File.Exists(categoryFile));
            Assert.True(File.Exists(notCategorizedFile));
            Assert.True(File.Exists(runSummaryFile));

            var categoryLines = await File.ReadAllLinesAsync(categoryFile);

            Assert.Equal(new[] { "a history", "z history" }, categoryLines);
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"heimdall-bragi-export-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
