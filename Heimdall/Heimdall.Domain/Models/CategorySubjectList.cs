namespace Heimdall.Domain.Models;

public sealed class CategorySubjectList
{
    public CategorySubjectList(CategoryDefinition category, IEnumerable<SubjectHeading> subjects)
    {
        Category = category;
        Subjects = subjects?.ToArray() ?? Array.Empty<SubjectHeading>();
        NormalizedSubjectValues = Subjects
            .Select(subject => subject.NormalizedValue)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public CategoryDefinition Category { get; }
    public IReadOnlyList<SubjectHeading> Subjects { get; }
    public IReadOnlySet<string> NormalizedSubjectValues { get; }
}
