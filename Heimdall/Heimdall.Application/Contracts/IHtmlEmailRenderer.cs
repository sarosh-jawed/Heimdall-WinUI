using Heimdall.Domain.Models;

namespace Heimdall.Application.Contracts;

public interface IHtmlEmailRenderer
{
    string RenderCategoryHtml(EmailCategoryPreview preview);
    string RenderCannotSortHtml(IReadOnlyList<EmailBookItem> cannotSortBooks);
}
