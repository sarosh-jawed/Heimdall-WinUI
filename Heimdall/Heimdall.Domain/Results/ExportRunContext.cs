using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Results;

public sealed class ExportRunContext
{
    public string SourceCsvPath { get; init; } = string.Empty;
    public string SubjectListMode { get; init; } = string.Empty;
    public string? SubjectListFolderPath { get; init; }
    public IReadOnlyList<CategoryKey> SelectedCategories { get; init; } = Array.Empty<CategoryKey>();
    public int TotalRecordsRead { get; init; }

    public IReadOnlyDictionary<CategoryKey, int> RemovedRecordCounts { get; init; } =
        new Dictionary<CategoryKey, int>();
}
