using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Models;

public sealed record RemovedBookSelection(CategoryKey CategoryKey, string BookId)
{
    public RemovedBookSelection(CategoryKey categoryKey, string bookId, string? reason = null)
        : this(categoryKey, bookId)
    {
        Reason = reason?.Trim() ?? string.Empty;
    }

    public string Reason { get; init; } = string.Empty;
}
