using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Application.Contracts;

public interface IBookCategoryMatcher
{
    CategoryMatchResult Match(
        IReadOnlyList<BookRecord> books,
        IReadOnlyList<CategorySubjectList> categorySubjectLists,
        IReadOnlyList<CategoryKey> selectedCategories);
}
