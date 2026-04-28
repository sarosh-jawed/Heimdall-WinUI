using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Matching;

public sealed class EmailPreviewBuilder : IEmailPreviewBuilder
{
    public EmailPreviewResult Build(CategoryMatchResult matchResult)
    {
        ArgumentNullException.ThrowIfNull(matchResult);

        var categoryPreviews = matchResult.CategorizedBooks
            .GroupBy(categorizedBook => categorizedBook.Category.Key.Value, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var category = group.First().Category;

                var books = group
                    .Select(categorizedBook => EmailBookItem.FromBook(categorizedBook.Book))
                    .GroupBy(book => book.BookId, StringComparer.OrdinalIgnoreCase)
                    .Select(bookGroup => bookGroup.First())
                    .OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(book => book.Author, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new EmailCategoryPreview(category, books);
            })
            .OrderBy(preview => preview.Category.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var cannotSortBooks = matchResult.CannotSortBooks
            .Select(EmailBookItem.FromBook)
            .GroupBy(book => book.BookId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(book => book.Author, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new EmailPreviewResult(
            categoryPreviews,
            cannotSortBooks,
            Array.Empty<string>());
    }
}
