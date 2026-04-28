namespace Heimdall.Application.Configuration;

public sealed class CsvColumnOptions
{
    public string TitleColumnName { get; init; } = "instances.title";
    public string AuthorColumnName { get; init; } = "instances.instance_primary_contributor";
    public string SummaryColumnName { get; init; } = "instances.notes";
    public string SubjectColumnName { get; init; } = "instances.subjects";
    public string RecordIdColumnName { get; init; } = "instances.id";

    public IReadOnlyList<string> RequiredColumnNames =>
        new[]
        {
            TitleColumnName,
            AuthorColumnName,
            SummaryColumnName,
            SubjectColumnName,
            RecordIdColumnName
        };
}
