namespace Heimdall.Application.Configuration;

public sealed class HeimdallConfig
{
    public CsvColumnOptions CsvColumns { get; init; } = new();
    public SubjectListOptions SubjectLists { get; init; } = new();
    public OutputOptions Output { get; init; } = new();
    public EmailTemplateOptions EmailTemplate { get; init; } = new();
    public BehaviorOptions Behavior { get; init; } = new();
    public LoggingOptions Logging { get; init; } = new();
}
