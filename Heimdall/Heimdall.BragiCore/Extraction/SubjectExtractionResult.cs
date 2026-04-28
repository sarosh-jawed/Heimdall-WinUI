namespace Heimdall.BragiCore.Extraction;

public sealed record SubjectExtractionResult(
    IReadOnlyList<ExtractedSubject> Subjects,
    int TotalRecordsRead,
    int BlankOrIgnoredCount,
    int DuplicateCount,
    IReadOnlyList<string> Warnings);
