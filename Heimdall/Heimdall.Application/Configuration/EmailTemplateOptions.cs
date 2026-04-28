namespace Heimdall.Application.Configuration;

public sealed class EmailTemplateOptions
{
    public string PageTitleTemplate { get; init; } = "New {{Category}} Books";
    public string HeaderTemplate { get; init; } = "New {{Category}} Books";
    public bool IncludeTitle { get; init; } = true;
    public bool IncludeAuthor { get; init; } = true;
    public bool IncludeSummary { get; init; } = true;
    public bool UseFixedHeader { get; init; } = true;
}
