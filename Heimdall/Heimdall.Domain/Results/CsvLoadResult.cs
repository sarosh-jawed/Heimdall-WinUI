using Heimdall.Domain.Models;

namespace Heimdall.Domain.Results;

public sealed record CsvLoadResult(
    IReadOnlyList<BookRecord> Books,
    int TotalRows,
    IReadOnlyList<string> Warnings)
{
    public static CsvLoadResult Empty { get; } =
        new(Array.Empty<BookRecord>(), 0, Array.Empty<string>());
}
