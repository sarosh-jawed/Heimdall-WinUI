namespace Heimdall.Domain.ValueObjects;

public sealed record OutputFileName
{
    public OutputFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Output file name cannot be blank.", nameof(value));
        }

        if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"Output file name contains invalid characters: {value}", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
