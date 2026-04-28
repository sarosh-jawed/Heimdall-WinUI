using System.Text;
using Heimdall.Application.Configuration;
using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Extraction;
using Heimdall.Infrastructure.Bragi;
using Heimdall.Infrastructure.Csv;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Bragi;

public sealed class BragiSubjectListGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_GeneratesFreshBragiSubjectListsWithoutExternalExe()
    {
        var csvPath = WriteTempCsv();
        var outputFolder = CreateTempFolder();

        try
        {
            var generator = CreateGenerator();

            var result = await generator.GenerateAsync(csvPath, outputFolder);

            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Combine(outputFolder, "HistorySubjects.txt")));
            Assert.True(File.Exists(Path.Combine(outputFolder, "EducationSubjects.txt")));
            Assert.True(File.Exists(Path.Combine(outputFolder, "NotCategorizedSubjects.txt")));
            Assert.True(File.Exists(Path.Combine(outputFolder, "RunSummary.txt")));

            var historyLines = await File.ReadAllLinesAsync(Path.Combine(outputFolder, "HistorySubjects.txt"));

            Assert.Equal(new[] { "History", "History of education" }, historyLines);
            Assert.NotNull(result.SubjectListLoadResult);
            Assert.Contains(result.SubjectListLoadResult.CategorySubjectLists, list =>
                list.Category.DisplayName == "History");
        }
        finally
        {
            File.Delete(csvPath);
            Directory.Delete(outputFolder, recursive: true);
        }
    }

    private static BragiSubjectListGenerator CreateGenerator()
    {
        var heimdallConfig = new HeimdallConfig();
        var schemaValidator = new CsvSchemaValidator(heimdallConfig);
        var summaryExtractor = new SummaryExtractor();
        var csvReader = new CsvBookRecordReader(heimdallConfig, schemaValidator, summaryExtractor);

        return new BragiSubjectListGenerator(
            csvReader,
            new BragiCoreOptions(),
            new SubjectExtractionService(),
            new CategorizationService(),
            new TextExportService());
    }

    private static string WriteTempCsv()
    {
        var content = string.Join(
            Environment.NewLine,
            "instances.id,instances.title,instances.instance_primary_contributor,instances.notes,instances.subjects",
            CsvLine("instance-001", "History Book", "Doe, Jane", "", "History; History of education; History"),
            CsvLine("instance-002", "Unknown Book", "Smith, John", "", "Unmatched specialized subject"));

        var path = Path.Combine(Path.GetTempPath(), $"heimdall-bragi-generator-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
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

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"heimdall-bragi-generator-output-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
