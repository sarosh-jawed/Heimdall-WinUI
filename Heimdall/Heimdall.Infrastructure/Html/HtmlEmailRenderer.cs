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

        string pageTitle = $"New {preview.Category.DisplayName} Books";

        return RenderBookTable(
            pageTitle,
            preview.ActiveBooks);
    }

    public string RenderCannotSortHtml(IReadOnlyList<EmailBookItem> cannotSortBooks)
    {
        ArgumentNullException.ThrowIfNull(cannotSortBooks);

        return RenderBookTable(
            "Cannot Sort Books",
            cannotSortBooks);
    }

    private static string RenderBookTable(
        string pageTitle,
        IReadOnlyList<EmailBookItem> books)
    {
        StringBuilder builder = new();

        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\" />");
        builder.AppendLine($"  <title>{Encode(pageTitle)}</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body { font-family: Segoe UI, Arial, sans-serif; color: #1f1f1f; line-height: 1.45; margin: 24px; }");
        builder.AppendLine("    h1 { font-size: 24px; margin-bottom: 18px; }");
        builder.AppendLine("    table { border-collapse: collapse; width: 100%; max-width: 1100px; }");
        builder.AppendLine("    th, td { border: 1px solid #d6d6d6; padding: 10px; vertical-align: top; text-align: left; }");
        builder.AppendLine("    th { background-color: #f3f3f3; font-weight: 600; }");
        builder.AppendLine("    .title-cell { font-weight: 600; }");
        builder.AppendLine("    .author-cell { width: 22%; }");
        builder.AppendLine("    .summary-cell { width: 48%; }");
        builder.AppendLine("    .empty-message { color: #666666; }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine($"  <h1>{Encode(pageTitle)}</h1>");

        if (books.Count == 0)
        {
            builder.AppendLine("  <p class=\"empty-message\">No books were selected for this category.</p>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        // These files are intended for manual copy/paste into an email draft,
        // so the markup stays simple and friendly to common email clients.
        builder.AppendLine("  <table>");
        builder.AppendLine("    <thead>");
        builder.AppendLine("      <tr>");
        builder.AppendLine("        <th>Title</th>");
        builder.AppendLine("        <th>Author</th>");
        builder.AppendLine("        <th>Summary</th>");
        builder.AppendLine("      </tr>");
        builder.AppendLine("    </thead>");
        builder.AppendLine("    <tbody>");

        foreach (EmailBookItem book in books)
        {
            builder.AppendLine("      <tr>");
            builder.AppendLine($"        <td class=\"title-cell\">{Encode(book.Title)}</td>");
            builder.AppendLine($"        <td class=\"author-cell\">{Encode(FormatOptionalValue(book.Author, "Not provided"))}</td>");
            builder.AppendLine($"        <td class=\"summary-cell\">{Encode(FormatOptionalValue(book.Summary, "No summary available."))}</td>");
            builder.AppendLine("      </tr>");
        }

        builder.AppendLine("    </tbody>");
        builder.AppendLine("  </table>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private static string FormatOptionalValue(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string Encode(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
