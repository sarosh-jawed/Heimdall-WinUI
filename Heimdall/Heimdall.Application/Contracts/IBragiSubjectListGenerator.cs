using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface IBragiSubjectListGenerator
{
    Task<BragiGenerationResult> GenerateAsync(
        string sourceCsvPath,
        string outputFolder,
        CancellationToken cancellationToken = default);
}
