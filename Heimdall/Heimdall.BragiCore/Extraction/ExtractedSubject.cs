namespace Heimdall.BragiCore.Extraction;

public sealed record ExtractedSubject(
    string OriginalSubject,
    string NormalizedSubject,
    string? SourceTitle,
    string? SourceRecordId,
    int SourceRowNumber);
