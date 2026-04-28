using Heimdall.Domain.Models;

namespace Heimdall.Domain.Results;

public sealed record EmailPreviewResult(
    IReadOnlyList<EmailCategoryPreview> Categories,
    IReadOnlyList<EmailBookItem> CannotSortBooks,
    IReadOnlyList<string> Warnings);
