using System.Text.RegularExpressions;

namespace Heimdall.Domain.ValueObjects;

public sealed record CategoryKey
{
    public CategoryKey(string value)
    {
        Value = Normalize(value);

        if (string.IsNullOrWhiteSpace(Value))
        {
            throw new ArgumentException("Category key cannot be blank.", nameof(value));
        }
    }

    public string Value { get; }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value.Trim(), @"\s+", " ").ToLowerInvariant();
    }

    public override string ToString() => Value;
}
