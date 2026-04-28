using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface IEmailPreviewBuilder
{
    EmailPreviewResult Build(CategoryMatchResult matchResult);
}
