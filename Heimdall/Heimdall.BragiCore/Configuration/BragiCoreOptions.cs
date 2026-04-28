namespace Heimdall.BragiCore.Configuration;

public sealed class BragiCoreOptions
{
    public CsvColumns CsvColumns { get; init; } = new();
    public BehaviorOptions BehaviorOptions { get; init; } = new();
    public IReadOnlyList<CategoryRule> CategoryRules { get; init; } = DefaultCategoryRules.Create();

    public string UncategorizedFileName { get; init; } = "NotCategorizedSubjects.txt";
    public string RunSummaryFileName { get; init; } = "RunSummary.txt";
}
