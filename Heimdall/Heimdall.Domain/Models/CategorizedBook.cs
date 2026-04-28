namespace Heimdall.Domain.Models;

public sealed record CategorizedBook(CategoryDefinition Category, BookRecord Book);
