using Heimdall.Domain.Models;

namespace Heimdall.Domain.Results;

public sealed record SubjectListLoadResult(
    IReadOnlyList<CategorySubjectList> CategorySubjectLists,
    IReadOnlyList<string> Warnings);
