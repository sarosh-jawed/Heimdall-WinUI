namespace Heimdall.BragiCore.Categorization;

public sealed record SubjectCategorizationResult(
    IReadOnlyList<SubjectCategoryAssignment> Assignments,
    IReadOnlyList<string> NotCategorizedSubjects,
    IReadOnlyDictionary<string, int> CategoryCounts);
