using System.Text.Json;
using System.Text.RegularExpressions;
using Heimdall.Application.Contracts;

namespace Heimdall.Infrastructure.Html;

/// <summary>
/// Conservative summary extraction for the first implementation pass.
/// John still needs to confirm the exact FOLIO note marker that should become the email Summary.
/// </summary>
public sealed class SummaryExtractor : ISummaryExtractor
{
    public string Extract(string rawNotes)
    {
        if (string.IsNullOrWhiteSpace(rawNotes))
        {
            return string.Empty;
        }

        var trimmedNotes = rawNotes.Trim();

        try
        {
            using var document = JsonDocument.Parse(trimmedNotes);
            var candidates = new List<NoteCandidate>();

            CollectCandidates(document.RootElement, candidates, inheritedStaffOnly: false, inheritedHint: string.Empty);

            return candidates
                .Where(candidate => !candidate.IsStaffOnly)
                .Select(candidate => CleanSummaryText(candidate.Text))
                .FirstOrDefault(IsSummaryLike)
                ?? string.Empty;
        }
        catch (JsonException)
        {
            return ExtractFromPlainText(trimmedNotes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractFromPlainText(string rawNotes)
    {
        var cleaned = CleanSummaryText(rawNotes);

        return IsSummaryLike(cleaned)
            ? cleaned
            : string.Empty;
    }

    private static void CollectCandidates(
        JsonElement element,
        ICollection<NoteCandidate> candidates,
        bool inheritedStaffOnly,
        string inheritedHint)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectCandidates(item, candidates, inheritedStaffOnly, inheritedHint);
                }

                break;

            case JsonValueKind.Object:
                var staffOnly = inheritedStaffOnly;
                var hint = inheritedHint;

                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("staffOnly") && property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    {
                        staffOnly = property.Value.GetBoolean();
                    }

                    if (property.Name.Contains("type", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("name", StringComparison.OrdinalIgnoreCase))
                    {
                        hint = $"{hint} {ReadStringValue(property.Value)}".Trim();
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (IsNoteTextProperty(property.Name) && property.Value.ValueKind == JsonValueKind.String)
                    {
                        var text = property.Value.GetString();

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            candidates.Add(new NoteCandidate(text, staffOnly, hint));
                        }
                    }
                    else
                    {
                        CollectCandidates(property.Value, candidates, staffOnly, hint);
                    }
                }

                break;
        }
    }

    private static bool IsNoteTextProperty(string propertyName)
    {
        return propertyName.Equals("note", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("text", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("content", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("value", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadStringValue(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string CleanSummaryText(string value)
    {
        var normalized = Regex.Replace(value.Trim(), @"\s+", " ");

        return Regex.Replace(
            normalized,
            @"^(summary|annotation|description)\s*:\s*",
            string.Empty,
            RegexOptions.IgnoreCase).Trim();
    }

    private static bool IsSummaryLike(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var lower = value.ToLowerInvariant();

        return lower.Contains("summary")
            || lower.Contains("annotation")
            || lower.Contains("description")
            || lower.Contains("publisher's description")
            || lower.Contains("publisher description")
            || value.Length >= 80 && LooksLikePublicDescription(value);
    }

    private static bool LooksLikePublicDescription(string value)
    {
        var lower = value.ToLowerInvariant();

        if (lower.Contains("staff")
            || lower.Contains("internal")
            || lower.Contains("barcode")
            || lower.Contains("call number")
            || lower.Contains("holdings")
            || lower.Contains("inventory"))
        {
            return false;
        }

        return value.Contains('.') || value.Contains(';');
    }

    private sealed record NoteCandidate(string Text, bool IsStaffOnly, string Hint);
}
