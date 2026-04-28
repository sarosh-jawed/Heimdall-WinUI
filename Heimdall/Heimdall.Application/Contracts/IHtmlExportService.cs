using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface IHtmlExportService
{
    Task<HtmlExportResult> ExportAsync(
        EmailPreviewResult previewResult,
        string outputFolder,
        CancellationToken cancellationToken = default);
}
