using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Application.Errors;

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
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvEmpty,
                "CSV header missing",
                "The selected CSV file does not contain a header row.",
                "Export a fresh official FOLIO CSV and try again.");
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
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvMissingRequiredColumns,
                "CSV is missing required columns",
                "The selected CSV file does not match the expected official FOLIO export format.",
                "Missing column(s): " + string.Join(", ", missingColumns) + ". Export the CSV again with the required FOLIO columns.");
        }
    }
}
