using System.Net;
using System.Text;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;

namespace Heimdall.Infrastructure.Html;

public sealed class HtmlEmailRenderer : IHtmlEmailRenderer
{
    public string RenderCategoryHtml(EmailCategoryPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        return RenderBookList(
            $"New {preview.Category.DisplayName} Books",
            preview.ActiveBooks);
    }

    public string RenderCannotSortHtml(IReadOnlyList<EmailBookItem> cannotSortBooks)
    {
        ArgumentNullException.ThrowIfNull(cannotSortBooks);

        return RenderBookList(
            "Cannot Sort Books",
            cannotSortBooks);
    }

    private static string RenderBookList(
        string title,
        IReadOnlyList<EmailBookItem> books)
    {
        var builder = new StringBuilder();

        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine($"  <title>{Encode(title)}</title>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine($"  <h1>{Encode(title)}</h1>");

        if (books.Count == 0)
        {
            builder.AppendLine("  <p>No books were selected for this category.</p>");
        }
        else
        {
            builder.AppendLine("  <ul>");

            foreach (var book in books)
            {
                builder.AppendLine("    <li>");
                builder.AppendLine($"      <strong>{Encode(book.Title)}</strong><br />");

                if (!string.IsNullOrWhiteSpace(book.Author))
                {
                    builder.AppendLine($"      <em>{Encode(book.Author)}</em><br />");
                }

                if (!string.IsNullOrWhiteSpace(book.Summary))
                {
                    builder.AppendLine($"      <p>{Encode(book.Summary)}</p>");
                }

                builder.AppendLine("    </li>");
            }

            builder.AppendLine("  </ul>");
        }

        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private static string Encode(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
