namespace Heimdall.Domain.Models;

public sealed class BookRecord
{
    public BookRecord(
        string? instanceId,
        string? title,
        string? author = null,
        string? summary = null,
        string? rawNotes = null,
        string? rawSubjects = null,
        IEnumerable<SubjectHeading>? subjectHeadings = null,
        IReadOnlyDictionary<string, string>? additionalFields = null)
    {
        if (string.IsNullOrWhiteSpace(instanceId) && string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A book record must have at least a title or an instance ID.");
        }

        InstanceId = instanceId?.Trim() ?? string.Empty;
        Title = title?.Trim() ?? string.Empty;
        Author = author?.Trim() ?? string.Empty;
        Summary = summary?.Trim() ?? string.Empty;
        RawNotes = rawNotes ?? string.Empty;
        RawSubjects = rawSubjects ?? string.Empty;
        SubjectHeadings = subjectHeadings?.ToArray() ?? Array.Empty<SubjectHeading>();

        AdditionalFields = additionalFields is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(additionalFields, StringComparer.OrdinalIgnoreCase);
    }

    public string InstanceId { get; }
    public string Title { get; }
    public string Author { get; }
    public string Summary { get; }
    public string RawNotes { get; }
    public string RawSubjects { get; }
    public IReadOnlyList<SubjectHeading> SubjectHeadings { get; }
    public IReadOnlyDictionary<string, string> AdditionalFields { get; }

    public bool HasSubjects => SubjectHeadings.Count > 0;
}
