using System.Text.RegularExpressions;
using Heimdall.BragiCore.Configuration;
using Heimdall.Domain.Models;

namespace Heimdall.BragiCore.Extraction;

public sealed class SubjectExtractionService
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public SubjectExtractionResult ExtractFromBookRecords(
        IReadOnlyList<BookRecord> books,
        BehaviorOptions behaviorOptions)
    {
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        var subjects = new List<ExtractedSubject>();
        var warnings = new List<string>();
        var seenSubjects = new HashSet<string>(StringComparer.Ordinal);
        var blankOrIgnoredCount = 0;
        var duplicateCount = 0;

        for (var index = 0; index < books.Count; index++)
        {
            var book = books[index];
            var sourceRowNumber = index + 2;

            var rawSubjects = book.SubjectHeadings.Count > 0
                ? book.SubjectHeadings.Select(subject => subject.RawValue)
                : SplitSubjectText(book.RawSubjects);

            var anySubjectForBook = false;

            foreach (var rawSubject in rawSubjects)
            {
                var preparedSubject = PrepareOriginalSubject(rawSubject, behaviorOptions);

                if (string.IsNullOrWhiteSpace(preparedSubject))
                {
                    blankOrIgnoredCount++;
                    continue;
                }

                anySubjectForBook = true;

                var normalizedSubject = PrepareNormalizedSubject(preparedSubject, behaviorOptions);

                if (!seenSubjects.Add(normalizedSubject))
                {
                    duplicateCount++;
                }

                subjects.Add(new ExtractedSubject(
                    preparedSubject,
                    normalizedSubject,
                    book.Title,
                    book.InstanceId,
                    sourceRowNumber));
            }

            if (!anySubjectForBook && !string.IsNullOrWhiteSpace(book.Title))
            {
                warnings.Add($"Book '{book.Title}' did not contain usable subject headings.");
            }
        }

        return new SubjectExtractionResult(
            subjects,
            books.Count,
            blankOrIgnoredCount,
            duplicateCount,
            warnings);
    }

    private static IReadOnlyList<string> SplitSubjectText(string rawSubjects)
    {
        if (string.IsNullOrWhiteSpace(rawSubjects))
        {
            return Array.Empty<string>();
        }

        return rawSubjects
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .ToArray();
    }

    private static string PrepareOriginalSubject(string? value, BehaviorOptions behaviorOptions)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var preparedValue = value;

        if (behaviorOptions.NormalizeWhitespace)
        {
            preparedValue = WhitespaceRegex.Replace(preparedValue, " ");
        }

        if (behaviorOptions.TrimSubjects)
        {
            preparedValue = preparedValue.Trim();
        }

        return preparedValue;
    }

    private static string PrepareNormalizedSubject(string originalValue, BehaviorOptions behaviorOptions)
    {
        var normalizedValue = originalValue;

        if (behaviorOptions.NormalizeWhitespace)
        {
            normalizedValue = WhitespaceRegex.Replace(normalizedValue, " ");
        }

        normalizedValue = normalizedValue.Trim();

        if (behaviorOptions.CaseInsensitiveMatching)
        {
            normalizedValue = normalizedValue.ToLowerInvariant();
        }

        return normalizedValue;
    }
}
