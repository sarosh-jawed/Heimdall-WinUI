using System.Text.RegularExpressions;

namespace Heimdall.Domain.Models;

public sealed record SubjectHeading
{
    public SubjectHeading(string rawValue)
    {
        RawValue = rawValue?.Trim() ?? string.Empty;
        NormalizedValue = Normalize(rawValue);

        if (string.IsNullOrWhiteSpace(NormalizedValue))
        {
            throw new ArgumentException("Subject heading cannot be blank.", nameof(rawValue));
        }
    }

    public string RawValue { get; }
    public string NormalizedValue { get; }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value.Trim(), @"\s+", " ").ToLowerInvariant();
    }

    public override string ToString() => RawValue;
}
