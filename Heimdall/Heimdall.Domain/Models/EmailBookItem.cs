namespace Heimdall.Domain.Models;

public sealed record EmailBookItem(
    string BookId,
    string Title,
    string Author,
    string Summary)
{
    public static EmailBookItem FromBook(BookRecord book)
    {
        return new EmailBookItem(
            string.IsNullOrWhiteSpace(book.InstanceId) ? book.Title : book.InstanceId,
            book.Title,
            book.Author,
            book.Summary);
    }
}
