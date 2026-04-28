namespace Heimdall.BragiCore.Configuration;

public sealed class CategoryRule
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string OutputFileName { get; init; } = string.Empty;
    public IReadOnlyList<string> IncludeKeywords { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ExcludeKeywords { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequireAnyKeywords { get; init; } = Array.Empty<string>();
    public bool DisableForFiction { get; init; }
    public bool DisableForJuvenile { get; init; }
    public int SortOrder { get; init; }
    public bool Enabled { get; init; } = true;
}
