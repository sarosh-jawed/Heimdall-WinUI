using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Infrastructure.Matching;

public sealed class BookCategoryMatcher : IBookCategoryMatcher
{
    public CategoryMatchResult Match(
        IReadOnlyList<BookRecord> books,
        IReadOnlyList<CategorySubjectList> categorySubjectLists,
        IReadOnlyList<CategoryKey> selectedCategories)
    {
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(categorySubjectLists);
        ArgumentNullException.ThrowIfNull(selectedCategories);

        var selectedCategoryKeys = selectedCategories
            .Select(category => category.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var activeCategorySubjectLists = categorySubjectLists
            .Where(categoryList => selectedCategoryKeys.Contains(categoryList.Category.Key.Value))
            .ToArray();

        var categorizedBooks = new List<CategorizedBook>();
        var cannotSortBooks = new List<BookRecord>();
        var categoryCounts = activeCategorySubjectLists
            .ToDictionary(
                categoryList => categoryList.Category.Key,
                _ => 0);

        foreach (var book in books)
        {
            var normalizedBookSubjects = GetNormalizedBookSubjects(book);

            if (normalizedBookSubjects.Count == 0)
            {
                cannotSortBooks.Add(book);
                continue;
            }

            var matchedAtLeastOneCategory = false;

            foreach (var categorySubjectList in activeCategorySubjectLists)
            {
                // Match the book to each selected broad category independently.
                // This is intentionally not first-match-wins because John confirmed
                // that a book can belong to more than one selected category.
                var matchesThisCategory = normalizedBookSubjects.Any(subject =>
                    categorySubjectList.NormalizedSubjectValues.Contains(subject));

                if (!matchesThisCategory)
                {
                    continue;
                }

                categorizedBooks.Add(new CategorizedBook(categorySubjectList.Category, book));
                categoryCounts[categorySubjectList.Category.Key]++;
                matchedAtLeastOneCategory = true;
            }

            if (!matchedAtLeastOneCategory)
            {
                cannotSortBooks.Add(book);
            }
        }

        return new CategoryMatchResult(
            categorizedBooks,
            cannotSortBooks,
            categoryCounts);
    }

    private static IReadOnlySet<string> GetNormalizedBookSubjects(BookRecord book)
    {
        var sourceSubjects = book.SubjectHeadings.Count > 0
            ? book.SubjectHeadings.Select(subject => subject.RawValue)
            : SplitRawSubjects(book.RawSubjects);

        return sourceSubjects
            .Select(SubjectHeading.Normalize)
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> SplitRawSubjects(string rawSubjects)
    {
        if (string.IsNullOrWhiteSpace(rawSubjects))
        {
            return Array.Empty<string>();
        }

        return rawSubjects
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .ToArray();
    }
}
