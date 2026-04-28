using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Bragi;

public sealed class SubjectListFolderReader : ISubjectListFolderReader
{
    private readonly ICategoryFileDetector _categoryFileDetector;

    public SubjectListFolderReader(ICategoryFileDetector categoryFileDetector)
    {
        _categoryFileDetector = categoryFileDetector;
    }

    public async Task<SubjectListLoadResult> ReadAsync(
        string subjectListFolder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subjectListFolder))
        {
            throw new ArgumentException("Subject-list folder path cannot be blank.", nameof(subjectListFolder));
        }

        if (!Directory.Exists(subjectListFolder))
        {
            throw new DirectoryNotFoundException("The selected Bragi subject-list folder was not found.");
        }

        var detectionResult = _categoryFileDetector.Detect(subjectListFolder);
        var categorySubjectLists = new List<CategorySubjectList>();
        var warnings = new List<string>(detectionResult.Warnings);

        foreach (var category in detectionResult.Categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = Path.Combine(subjectListFolder, category.SubjectFileName.Value);

            if (!File.Exists(filePath))
            {
                warnings.Add($"Subject-list file was detected but could not be read: {category.SubjectFileName.Value}");
                continue;
            }

            var subjects = await ReadSubjectHeadingsAsync(filePath, warnings, cancellationToken);

            categorySubjectLists.Add(new CategorySubjectList(category, subjects));
        }

        if (categorySubjectLists.Count == 0)
        {
            throw new InvalidOperationException("No Bragi subject-list files could be read from the selected folder.");
        }

        return new SubjectListLoadResult(categorySubjectLists, warnings);
    }

    private static async Task<IReadOnlyList<SubjectHeading>> ReadSubjectHeadingsAsync(
        string filePath,
        ICollection<string> warnings,
        CancellationToken cancellationToken)
    {
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);

        var preparedLines = lines
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .GroupBy(line => line, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(line => line, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var subjects = new List<SubjectHeading>();

        foreach (var line in preparedLines)
        {
            try
            {
                subjects.Add(new SubjectHeading(line));
            }
            catch (ArgumentException)
            {
                warnings.Add($"A blank or invalid subject heading was ignored in {Path.GetFileName(filePath)}.");
            }
        }

        return subjects;
    }
}
