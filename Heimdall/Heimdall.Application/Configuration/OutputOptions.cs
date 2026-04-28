namespace Heimdall.Application.Configuration;

public sealed class OutputOptions
{
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public string CategoryFileNameTemplate { get; init; } = "{{Category}}NewBooks{{Date}}.html";
    public string CannotSortFileNameTemplate { get; init; } = "CannotSortBooks{{Date}}.html";
    public string RunSummaryFileNameTemplate { get; init; } = "RunSummary{{Date}}.txt";
    public bool SaveDirectlyIntoSelectedFolder { get; init; } = true;
}
