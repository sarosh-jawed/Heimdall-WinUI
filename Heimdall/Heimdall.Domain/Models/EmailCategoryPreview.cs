namespace Heimdall.Domain.Models;

public sealed class EmailCategoryPreview
{
    public EmailCategoryPreview(
        CategoryDefinition category,
        IEnumerable<EmailBookItem> books,
        IEnumerable<string>? removedBookIds = null)
    {
        Category = category;
        Books = books?.ToArray() ?? Array.Empty<EmailBookItem>();
        RemovedBookIds = removedBookIds?.ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public CategoryDefinition Category { get; }
    public IReadOnlyList<EmailBookItem> Books { get; }
    public IReadOnlySet<string> RemovedBookIds { get; }

    public IReadOnlyList<EmailBookItem> ActiveBooks =>
        Books.Where(book => !RemovedBookIds.Contains(book.BookId)).ToArray();

    public int ActiveBookCount => ActiveBooks.Count;
}
