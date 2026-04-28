using Heimdall.Domain.Models;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Results;

public sealed record CategoryMatchResult(
    IReadOnlyList<CategorizedBook> CategorizedBooks,
    IReadOnlyList<BookRecord> CannotSortBooks,
    IReadOnlyDictionary<CategoryKey, int> CategoryCounts);
