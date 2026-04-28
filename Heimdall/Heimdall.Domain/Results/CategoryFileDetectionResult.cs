using Heimdall.Domain.Models;

namespace Heimdall.Domain.Results;

public sealed record CategoryFileDetectionResult(
    IReadOnlyList<CategoryDefinition> Categories,
    IReadOnlyList<string> MissingExpectedFiles,
    IReadOnlyList<string> Warnings)
{
    public bool HasCategories => Categories.Count > 0;
}
