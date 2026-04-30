using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Application.Errors;

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
            throw new UserFriendlyException(
                HeimdallErrorCode.SubjectListFolderBlank,
                "Existing Bragi folder required",
                "No existing Bragi subject-list folder was selected.",
                "Select the Bragi output folder and try again.");
        }

        if (!Directory.Exists(subjectListFolder))
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.SubjectListFolderNotFound,
                "Existing Bragi folder not found",
                "The selected Bragi subject-list folder could not be found.",
                "Select the folder again.");
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
            throw new UserFriendlyException(
                HeimdallErrorCode.SubjectListFilesUnreadable,
                "Bragi subject files could not be read",
                "Heimdall found the folder, but no usable Bragi subject-list files could be read.",
                "Make sure the folder contains readable files such as ArtSubjects.txt or FictionSubjects.txt.");
        }

        return new SubjectListLoadResult(categorySubjectLists, warnings);
    }

    private static async Task<IReadOnlyList<SubjectHeading>> ReadSubjectHeadingsAsync(
        string filePath,
        ICollection<string> warnings,
        CancellationToken cancellationToken)
    {
        string[] lines;

        try
        {
            lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.SubjectListFilesUnreadable,
                "Bragi subject file cannot be accessed",
                $"Heimdall does not have permission to read {Path.GetFileName(filePath)}.",
                "Check the folder permissions or select a different Bragi output folder.",
                ex);
        }
        catch (IOException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.SubjectListFilesUnreadable,
                "Bragi subject file unavailable",
                $"Heimdall could not read {Path.GetFileName(filePath)}.",
                "Close the file if it is open in another program, then try again.",
                ex);
        }

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
