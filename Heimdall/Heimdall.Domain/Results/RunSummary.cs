using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Results;

public sealed class RunSummary
{
    public DateTimeOffset RunStartedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? RunEndedAt { get; init; }
    public string SourceCsvPath { get; init; } = string.Empty;
    public string SubjectListMode { get; init; } = string.Empty;
    public string? SubjectListFolderPath { get; init; }
    public IReadOnlyList<CategoryKey> SelectedCategories { get; init; } = Array.Empty<CategoryKey>();
    public int TotalRecordsRead { get; init; }

    public IReadOnlyDictionary<CategoryKey, int> MatchedRecordCounts { get; init; } =
        new Dictionary<CategoryKey, int>();

    public IReadOnlyDictionary<CategoryKey, int> RemovedRecordCounts { get; init; } =
        new Dictionary<CategoryKey, int>();

    public int CannotSortCount { get; init; }
    public IReadOnlyList<OutputFileName> GeneratedFiles { get; init; } = Array.Empty<OutputFileName>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    public TimeSpan? Duration => RunEndedAt is null
        ? null
        : RunEndedAt.Value - RunStartedAt;
}
