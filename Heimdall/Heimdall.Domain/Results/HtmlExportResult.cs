using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Results;

public sealed record HtmlExportResult(
    string OutputFolder,
    IReadOnlyList<OutputFileName> GeneratedFiles,
    RunSummary RunSummary,
    IReadOnlyList<string> Warnings);
