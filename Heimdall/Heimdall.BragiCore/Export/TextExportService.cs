using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;

namespace Heimdall.BragiCore.Export;

public sealed class TextExportService
{
    public async Task<BragiTextExportResult> ExportAsync(
        string outputFolder,
        IReadOnlyList<CategoryRule> categoryRules,
        SubjectCategorizationResult categorizationResult,
        BragiCoreOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFolder);
        ArgumentNullException.ThrowIfNull(categoryRules);
        ArgumentNullException.ThrowIfNull(categorizationResult);
        ArgumentNullException.ThrowIfNull(options);

        Directory.CreateDirectory(outputFolder);

        var generatedFiles = new List<string>();
        var exportedCategorySubjects = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in categoryRules.Where(rule => rule.Enabled).OrderBy(rule => rule.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var categorySubjects = categorizationResult.Assignments
                .Where(assignment => assignment.CategoryRule.Key.Equals(rule.Key, StringComparison.OrdinalIgnoreCase))
                .Select(assignment => assignment.Subject.OriginalSubject);

            var preparedSubjects = PrepareOutputLines(categorySubjects, options.BehaviorOptions);

            var filePath = Path.Combine(outputFolder, rule.OutputFileName);
            await File.WriteAllLinesAsync(filePath, preparedSubjects, cancellationToken);

            generatedFiles.Add(filePath);
            exportedCategorySubjects[rule.Key] = preparedSubjects;
        }

        var notCategorizedSubjects = PrepareOutputLines(
            categorizationResult.NotCategorizedSubjects,
            options.BehaviorOptions);

        var notCategorizedPath = Path.Combine(outputFolder, options.UncategorizedFileName);
        await File.WriteAllLinesAsync(notCategorizedPath, notCategorizedSubjects, cancellationToken);

        generatedFiles.Add(notCategorizedPath);

        var runSummaryPath = Path.Combine(outputFolder, options.RunSummaryFileName);
        await File.WriteAllLinesAsync(
            runSummaryPath,
            BuildRunSummaryLines(categoryRules, categorizationResult, notCategorizedSubjects.Count),
            cancellationToken);

        generatedFiles.Add(runSummaryPath);

        return new BragiTextExportResult(
            outputFolder,
            exportedCategorySubjects,
            notCategorizedSubjects,
            generatedFiles);
    }

    private static IReadOnlyList<string> PrepareOutputLines(
        IEnumerable<string> subjects,
        BehaviorOptions behaviorOptions)
    {
        var query = subjects
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .Select(subject => subject.Trim());

        if (behaviorOptions.DeduplicateOutputs)
        {
            query = query
                .GroupBy(subject => subject, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First());
        }

        var output = query.ToList();

        if (behaviorOptions.SortOutputs)
        {
            output.Sort(StringComparer.OrdinalIgnoreCase);
        }

        return output;
    }

    private static IReadOnlyList<string> BuildRunSummaryLines(
        IReadOnlyList<CategoryRule> categoryRules,
        SubjectCategorizationResult categorizationResult,
        int notCategorizedCount)
    {
        var lines = new List<string>
        {
            "Bragi Run Summary",
            $"Generated At: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}",
            $"Total Assignments: {categorizationResult.Assignments.Count}",
            $"Not Categorized Subjects: {notCategorizedCount}",
            string.Empty,
            "Category Counts:"
        };

        foreach (var rule in categoryRules.Where(rule => rule.Enabled).OrderBy(rule => rule.SortOrder))
        {
            var count = categorizationResult.CategoryCounts.TryGetValue(rule.Key, out var value)
                ? value
                : 0;

            lines.Add($"{rule.Key}: {count}");
        }

        return lines;
    }
}
