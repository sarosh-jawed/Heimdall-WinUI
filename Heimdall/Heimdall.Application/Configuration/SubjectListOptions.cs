namespace Heimdall.Application.Configuration;

public sealed class SubjectListOptions
{
    public bool AllowGenerateFreshLists { get; init; } = true;
    public bool AllowExistingSubjectListFolder { get; init; } = true;
    public IReadOnlyList<string> RequiredCategoryFiles { get; init; } = Array.Empty<string>();
}
