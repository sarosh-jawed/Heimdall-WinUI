using Heimdall.Domain.Models;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Html;

public sealed class HtmlEmailRendererTests
{
    [Fact]
    public void RenderCategoryHtml_ContainsTitleAuthorAndSummary()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("Art"),
            new[]
            {
                new EmailBookItem(
                    "book-001",
                    "The Art Book",
                    "Smith, Jane",
                    "A readable summary for the email preview.")
            });

        string html = renderer.RenderCategoryHtml(preview);

        Assert.Contains("New Art Books", html);
        Assert.Contains("The Art Book", html);
        Assert.Contains("Smith, Jane", html);
        Assert.Contains("A readable summary for the email preview.", html);
        Assert.Contains("<table>", html);
        Assert.Contains("<th>Title</th>", html);
        Assert.Contains("<th>Author</th>", html);
        Assert.Contains("<th>Summary</th>", html);
    }

    [Fact]
    public void RenderCategoryHtml_HtmlEncodesSpecialCharacters()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("Art & Design"),
            new[]
            {
                new EmailBookItem(
                    "book-002",
                    "A <Great> & \"Special\" Book",
                    "Jane <Editor> & Co.",
                    "Summary with <b>bold</b> & detail.")
            });

        string html = renderer.RenderCategoryHtml(preview);

        Assert.Contains("New Art &amp; Design Books", html);
        Assert.Contains("A &lt;Great&gt; &amp; &quot;Special&quot; Book", html);
        Assert.Contains("Jane &lt;Editor&gt; &amp; Co.", html);
        Assert.Contains("Summary with &lt;b&gt;bold&lt;/b&gt; &amp; detail.", html);

        Assert.DoesNotContain("A <Great>", html);
        Assert.DoesNotContain("Jane <Editor>", html);
        Assert.DoesNotContain("<b>bold</b>", html);
    }

    [Fact]
    public void RenderCategoryHtml_EmptyAuthorDoesNotCrash()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("History"),
            new[]
            {
                new EmailBookItem(
                    "book-003",
                    "Untitled Author Test",
                    string.Empty,
                    "Summary exists.")
            });

        string html = renderer.RenderCategoryHtml(preview);

        Assert.Contains("Untitled Author Test", html);
        Assert.Contains("Not provided", html);
        Assert.Contains("Summary exists.", html);
    }

    [Fact]
    public void RenderCategoryHtml_EmptySummaryDoesNotCrash()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("Business"),
            new[]
            {
                new EmailBookItem(
                    "book-004",
                    "Summary Missing Test",
                    "Business Author",
                    string.Empty)
            });

        string html = renderer.RenderCategoryHtml(preview);

        Assert.Contains("Summary Missing Test", html);
        Assert.Contains("Business Author", html);
        Assert.Contains("No summary available.", html);
    }

    [Fact]
    public void RenderCategoryHtml_RemovedBooksDoNotRender()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("Computer"),
            new[]
            {
                new EmailBookItem(
                    "removed-book",
                    "Removed Book Title",
                    "Removed Author",
                    "Removed summary."),
                new EmailBookItem(
                    "active-book",
                    "Active Book Title",
                    "Active Author",
                    "Active summary.")
            },
            removedBookIds: new[]
            {
                "removed-book"
            });

        string html = renderer.RenderCategoryHtml(preview);

        Assert.DoesNotContain("Removed Book Title", html);
        Assert.DoesNotContain("Removed Author", html);
        Assert.DoesNotContain("Removed summary.", html);

        Assert.Contains("Active Book Title", html);
        Assert.Contains("Active Author", html);
        Assert.Contains("Active summary.", html);
    }

    [Fact]
    public void RenderCannotSortHtml_ContainsCannotSortHeader()
    {
        HtmlEmailRenderer renderer = new();

        string html = renderer.RenderCannotSortHtml(
            new[]
            {
                new EmailBookItem(
                    "cannot-sort-001",
                    "Unmatched Book",
                    "Unknown Author",
                    "This book did not match a selected category.")
            });

        Assert.Contains("Cannot Sort Books", html);
        Assert.Contains("Unmatched Book", html);
        Assert.Contains("Unknown Author", html);
        Assert.Contains("This book did not match a selected category.", html);
    }

    [Fact]
    public void RenderCategoryHtml_EmptyBookListShowsFriendlyMessage()
    {
        HtmlEmailRenderer renderer = new();

        EmailCategoryPreview preview = new(
            CreateCategory("Psych"),
            Array.Empty<EmailBookItem>());

        string html = renderer.RenderCategoryHtml(preview);

        Assert.Contains("New Psych Books", html);
        Assert.Contains("No books were selected for this category.", html);
    }

    private static CategoryDefinition CreateCategory(string displayName)
    {
        return new CategoryDefinition(
            displayName,
            $"{displayName.Replace(" ", string.Empty)}Subjects.txt");
    }
}
