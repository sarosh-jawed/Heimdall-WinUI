using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;

namespace Heimdall.Infrastructure.Csv;

public sealed class CsvSchemaValidator : ICsvSchemaValidator
{
    private readonly HeimdallConfig _config;

    public CsvSchemaValidator(HeimdallConfig config)
    {
        _config = config;
    }

    public void ValidateOrThrow(IReadOnlyCollection<string> columnNames)
    {
        if (columnNames.Count == 0)
        {
            throw new InvalidOperationException("The selected CSV file does not contain a header row.");
        }

        var existingColumns = columnNames
            .Where(column => !string.IsNullOrWhiteSpace(column))
            .Select(column => column.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingColumns = _config.CsvColumns.RequiredColumnNames
            .Where(requiredColumn => !existingColumns.Contains(requiredColumn))
            .ToArray();

        if (missingColumns.Length > 0)
        {
            throw new InvalidOperationException(
                "The selected CSV file is missing required FOLIO column(s): "
                + string.Join(", ", missingColumns));
        }
    }
}
