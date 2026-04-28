using Heimdall.Domain.ValueObjects;

namespace Heimdall.Domain.Models;

public sealed record CategoryDefinition
{
    public CategoryDefinition(CategoryKey key, string displayName, OutputFileName subjectFileName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Category display name cannot be blank.", nameof(displayName));
        }

        Key = key;
        DisplayName = displayName.Trim();
        SubjectFileName = subjectFileName;
    }

    public CategoryDefinition(string displayName, string subjectFileName)
        : this(new CategoryKey(displayName), displayName, new OutputFileName(subjectFileName))
    {
    }

    public CategoryKey Key { get; }
    public string DisplayName { get; }
    public OutputFileName SubjectFileName { get; }
}
