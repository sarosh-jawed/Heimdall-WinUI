namespace Heimdall.Application.Configuration;

public sealed class BehaviorOptions
{
    public bool AllowMultiCategoryMatch { get; init; } = true;
    public bool GenerateCannotSortFile { get; init; } = true;
    public bool HtmlEncodeOutput { get; init; } = true;
    public bool TrimValues { get; init; } = true;
    public bool NormalizeWhitespace { get; init; } = true;
    public bool CaseInsensitiveSubjectMatching { get; init; } = true;
}
