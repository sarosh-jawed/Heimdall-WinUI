namespace Heimdall.BragiCore.Configuration;

public sealed class BehaviorOptions
{
    public bool AllowMultiMatch { get; init; } = true;
    public bool SortOutputs { get; init; } = true;
    public bool DeduplicateOutputs { get; init; } = true;
    public bool IgnoreBlankSubjects { get; init; } = true;
    public bool CaseInsensitiveMatching { get; init; } = true;
    public bool NormalizeWhitespace { get; init; } = true;
    public bool TrimSubjects { get; init; } = true;
}
