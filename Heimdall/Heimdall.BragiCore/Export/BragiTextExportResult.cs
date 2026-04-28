namespace Heimdall.BragiCore.Export;

public sealed record BragiTextExportResult(
    string OutputFolder,
    IReadOnlyDictionary<string, IReadOnlyList<string>> CategorySubjects,
    IReadOnlyList<string> NotCategorizedSubjects,
    IReadOnlyList<string> GeneratedFiles);
