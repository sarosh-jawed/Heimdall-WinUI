using Heimdall.Domain.Models;
using Heimdall.Domain.ValueObjects;
using Heimdall.Infrastructure.Matching;

namespace Heimdall.Tests.Matching;

public sealed class BookCategoryMatcherTests
{
    [Fact]
    public void Match_AddsBookWithArtSubjectToArtCategory()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Art Book", "Art")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("Art", "ArtSubjects.txt", "Art")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("Art") });

        Assert.Single(result.CategorizedBooks);
        Assert.Equal("Art", result.CategorizedBooks[0].Category.DisplayName);
        Assert.Empty(result.CannotSortBooks);
    }

    [Fact]
    public void Match_AddsBookWithHistorySubjectToHistoryCategory()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "History Book", "History")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("History", "HistorySubjects.txt", "History")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("History") });

        Assert.Single(result.CategorizedBooks);
        Assert.Equal("History", result.CategorizedBooks[0].Category.DisplayName);
        Assert.Empty(result.CannotSortBooks);
    }

    [Fact]
    public void Match_AddsBookToEveryMatchingSelectedCategory()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Education History Book", "History; Education")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("History", "HistorySubjects.txt", "History"),
            CreateCategorySubjectList("Education", "EducationSubjects.txt", "Education")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[]
            {
                new CategoryKey("History"),
                new CategoryKey("Education")
            });

        Assert.Equal(2, result.CategorizedBooks.Count);
        Assert.Contains(result.CategorizedBooks, book => book.Category.DisplayName == "History");
        Assert.Contains(result.CategorizedBooks, book => book.Category.DisplayName == "Education");
        Assert.Empty(result.CannotSortBooks);
    }

    [Fact]
    public void Match_AddsBookWithNoSubjectsToCannotSort()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            new BookRecord(
                instanceId: "book-001",
                title: "No Subject Book",
                rawSubjects: string.Empty,
                subjectHeadings: Array.Empty<SubjectHeading>())
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("Art", "ArtSubjects.txt", "Art")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("Art") });

        Assert.Empty(result.CategorizedBooks);
        Assert.Single(result.CannotSortBooks);
    }

    [Fact]
    public void Match_AddsBookWithUnmatchedSubjectToCannotSort()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Unmatched Book", "Completely Unknown Subject")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("Art", "ArtSubjects.txt", "Art")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("Art") });

        Assert.Empty(result.CategorizedBooks);
        Assert.Single(result.CannotSortBooks);
    }

    [Fact]
    public void Match_IsCaseInsensitive()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Case Test Book", "history")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("History", "HistorySubjects.txt", "HISTORY")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("history") });

        Assert.Single(result.CategorizedBooks);
        Assert.Equal("History", result.CategorizedBooks[0].Category.DisplayName);
    }

    [Fact]
    public void Match_TrimsAndNormalizesWhitespace()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Whitespace Test Book", "  Social    history  ")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("History", "HistorySubjects.txt", "Social history")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("History") });

        Assert.Single(result.CategorizedBooks);
    }

    [Fact]
    public void Match_DuplicateBookSubjectHeadingsDoNotDuplicateSameBookInSameCategory()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Duplicate Subject Book", "History; History; history")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("History", "HistorySubjects.txt", "History")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("History") });

        Assert.Single(result.CategorizedBooks);
        Assert.Equal(1, result.CategoryCounts[new CategoryKey("History")]);
    }

    [Fact]
    public void Match_OnlyUsesSelectedCategories()
    {
        var matcher = new BookCategoryMatcher();
        var books = new[]
        {
            CreateBook("book-001", "Art History Book", "Art; History")
        };
        var subjectLists = new[]
        {
            CreateCategorySubjectList("Art", "ArtSubjects.txt", "Art"),
            CreateCategorySubjectList("History", "HistorySubjects.txt", "History")
        };

        var result = matcher.Match(
            books,
            subjectLists,
            new[] { new CategoryKey("History") });

        Assert.Single(result.CategorizedBooks);
        Assert.Equal("History", result.CategorizedBooks[0].Category.DisplayName);
    }

    private static BookRecord CreateBook(
        string instanceId,
        string title,
        string rawSubjects)
    {
        var subjectHeadings = rawSubjects
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .Select(subject => new SubjectHeading(subject))
            .ToArray();

        return new BookRecord(
            instanceId: instanceId,
            title: title,
            rawSubjects: rawSubjects,
            subjectHeadings: subjectHeadings);
    }

    private static CategorySubjectList CreateCategorySubjectList(
        string displayName,
        string fileName,
        params string[] subjects)
    {
        return new CategorySubjectList(
            new CategoryDefinition(displayName, fileName),
            subjects.Select(subject => new SubjectHeading(subject)));
    }
}
