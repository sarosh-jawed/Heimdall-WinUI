using Heimdall.Application.Configuration;
using Heimdall.Application.Errors;
using Heimdall.Infrastructure.Export;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Export;

public sealed class ExportErrorHandlingTests
{
    [Fact]
    public async Task ExportAsync_ThrowsFriendlyError_ForBlankOutputFolder()
    {
        var service = new HtmlExportService(
            new HeimdallConfig(),
            new HtmlEmailRenderer(),
            new RunSummaryService());

        var preview = new Heimdall.Domain.Results.EmailPreviewResult(
            Array.Empty<Heimdall.Domain.Models.EmailCategoryPreview>(),
            Array.Empty<Heimdall.Domain.Models.EmailBookItem>(),
            Array.Empty<string>());

        var exception = await Assert.ThrowsAsync<UserFriendlyException>(
            () => service.ExportAsync(preview, string.Empty));

        Assert.Equal(HeimdallErrorCode.OutputFolderBlank, exception.ErrorCode);
        Assert.Equal("Output folder required", exception.Title);
    }
}
