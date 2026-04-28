using Heimdall.Application.Contracts;
using Heimdall.BragiCore.Configuration;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Bragi;

public sealed class CategoryFileDetector : ICategoryFileDetector
{
    private const string NoFilesFoundMessage =
        "No Bragi subject-list files were found in the selected folder.";

    private readonly BragiCoreOptions _options;

    public CategoryFileDetector(BragiCoreOptions options)
    {
        _options = options;
    }

    public CategoryFileDetectionResult Detect(string subjectListFolder)
    {
        if (string.IsNullOrWhiteSpace(subjectListFolder))
        {
            throw new ArgumentException("Subject-list folder path cannot be blank.", nameof(subjectListFolder));
        }

        if (!Directory.Exists(subjectListFolder))
        {
            throw new DirectoryNotFoundException("The selected Bragi subject-list folder was not found.");
        }

        var filesByName = Directory
            .EnumerateFiles(subjectListFolder, "*Subjects.txt", SearchOption.TopDirectoryOnly)
            .ToDictionary(Path.GetFileName, StringComparer.OrdinalIgnoreCase);

        var detectedCategories = new List<CategoryDefinition>();
        var missingExpectedFiles = new List<string>();
        var warnings = new List<string>();

        foreach (var rule in _options.CategoryRules.Where(rule => rule.Enabled).OrderBy(rule => rule.SortOrder))
        {
            if (filesByName.ContainsKey(rule.OutputFileName))
            {
                detectedCategories.Add(new CategoryDefinition(rule.DisplayName, rule.OutputFileName));
            }
            else
            {
                missingExpectedFiles.Add(rule.OutputFileName);
            }
        }

        AddUnknownSubjectFiles(filesByName.Keys, detectedCategories);

        if (detectedCategories.Count == 0)
        {
            throw new InvalidOperationException(NoFilesFoundMessage);
        }

        if (missingExpectedFiles.Count > 0)
        {
            warnings.Add(
                "Some expected Bragi subject-list files were not found: "
                + string.Join(", ", missingExpectedFiles));
        }

        return new CategoryFileDetectionResult(
            detectedCategories,
            missingExpectedFiles,
            warnings);
    }

    private static void AddUnknownSubjectFiles(
        IEnumerable<string?> fileNames,
        ICollection<CategoryDefinition> detectedCategories)
    {
        var existingFileNames = detectedCategories
            .Select(category => category.SubjectFileName.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var fileName in fileNames.Where(name => !string.IsNullOrWhiteSpace(name)))
        {
            if (fileName is null || existingFileNames.Contains(fileName))
            {
                continue;
            }

            if (IsNonCategoryBragiOutput(fileName))
            {
                continue;
            }

            var displayName = BuildDisplayNameFromFileName(fileName);

            detectedCategories.Add(new CategoryDefinition(displayName, fileName));
        }
    }

    private static bool IsNonCategoryBragiOutput(string fileName)
    {
        return fileName.Equals("NotCategorizedSubjects.txt", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("RunSummary.txt", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDisplayNameFromFileName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (nameWithoutExtension.EndsWith("Subjects", StringComparison.OrdinalIgnoreCase))
        {
            nameWithoutExtension = nameWithoutExtension[..^"Subjects".Length];
        }

        return nameWithoutExtension.ToLowerInvariant() switch
        {
            "slim" => "SLIM",
            "hper" => "HPER",
            "idt" => "IDT",
            "interdis" => "InterDis",
            _ => nameWithoutExtension
        };
    }
}
