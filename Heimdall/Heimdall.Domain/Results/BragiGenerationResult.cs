namespace Heimdall.Domain.Results;

public sealed record BragiGenerationResult(
    bool Success,
    string OutputFolder,
    SubjectListLoadResult? SubjectListLoadResult,
    IReadOnlyList<string> Warnings);
