using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Extraction;

namespace Heimdall.BragiCore.Categorization;

public sealed class CategorizationService
{
    public SubjectCategorizationResult Categorize(
        SubjectExtractionResult extractionResult,
        IReadOnlyList<CategoryRule> categoryRules,
        BehaviorOptions behaviorOptions)
    {
        ArgumentNullException.ThrowIfNull(extractionResult);
        ArgumentNullException.ThrowIfNull(categoryRules);
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        var enabledRules = categoryRules
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.SortOrder)
            .ThenBy(rule => rule.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var assignments = new List<SubjectCategoryAssignment>();
        var notCategorized = new List<string>();
        var categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var subject in extractionResult.Subjects)
        {
            var matchedAtLeastOneCategory = false;

            foreach (var rule in enabledRules)
            {
                if (!MatchesRule(subject.NormalizedSubject, rule))
                {
                    continue;
                }

                assignments.Add(new SubjectCategoryAssignment(
                    subject,
                    rule,
                    $"Matched configured keyword for {rule.DisplayName}."));

                categoryCounts[rule.Key] = categoryCounts.TryGetValue(rule.Key, out var currentCount)
                    ? currentCount + 1
                    : 1;

                matchedAtLeastOneCategory = true;

                if (!behaviorOptions.AllowMultiMatch)
                {
                    break;
                }
            }

            if (!matchedAtLeastOneCategory)
            {
                notCategorized.Add(subject.OriginalSubject);
            }
        }

        return new SubjectCategorizationResult(assignments, notCategorized, categoryCounts);
    }

    private static bool MatchesRule(string normalizedSubject, CategoryRule rule)
    {
        if (string.IsNullOrWhiteSpace(normalizedSubject))
        {
            return false;
        }

        if (rule.DisableForFiction && normalizedSubject.Contains("fiction", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (rule.DisableForJuvenile &&
            (normalizedSubject.Contains("juvenile", StringComparison.OrdinalIgnoreCase) ||
             normalizedSubject.Contains("children", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (rule.ExcludeKeywords.Any(keyword =>
                normalizedSubject.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!rule.IncludeKeywords.Any(keyword =>
                normalizedSubject.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (rule.RequireAnyKeywords.Count > 0 &&
            !rule.RequireAnyKeywords.Any(keyword =>
                normalizedSubject.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
