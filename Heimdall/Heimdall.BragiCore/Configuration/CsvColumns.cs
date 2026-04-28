namespace Heimdall.BragiCore.Configuration;

public sealed class CsvColumns
{
    public string SubjectColumnName { get; init; } = "instances.subjects";
    public string TitleColumnName { get; init; } = "instances.title";
    public string RecordIdColumnName { get; init; } = "instances.id";

    // The official Heimdall FOLIO file stores subjects as semicolon-separated text.
    // This remains configurable because Bragi also supports JSON-array subject payloads.
    public bool SubjectColumnContainsJsonArray { get; init; }
}
