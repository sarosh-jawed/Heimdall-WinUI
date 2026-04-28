using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Csv;

public sealed class CsvBookRecordReader : ICsvBookRecordReader
{
    private static readonly string[] OptionalColumnNames =
    {
        "instances.hrid",
        "items.barcode",
        "items.effective_call_number",
        "effective_location.name",
        "mtypes.name",
        "instances.publication"
    };

    private readonly HeimdallConfig _config;
    private readonly ICsvSchemaValidator _schemaValidator;
    private readonly ISummaryExtractor _summaryExtractor;

    public CsvBookRecordReader(
        HeimdallConfig config,
        ICsvSchemaValidator schemaValidator,
        ISummaryExtractor summaryExtractor)
    {
        _config = config;
        _schemaValidator = schemaValidator;
        _summaryExtractor = summaryExtractor;
    }

    public async Task<CsvLoadResult> ReadAsync(
        string csvPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(csvPath))
        {
            throw new ArgumentException("CSV path cannot be blank.", nameof(csvPath));
        }

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException("The selected FOLIO CSV file was not found.", csvPath);
        }

        var books = new List<BookRecord>();
        var warnings = new List<string>();

        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = context =>
            {
                warnings.Add($"Bad CSV data was found near row {context.Context.Parser.Row}.");
            },
            MissingFieldFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header?.Trim().ToLowerInvariant() ?? string.Empty
        };

        await using var stream = File.OpenRead(csvPath);
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, csvConfiguration);

        if (!await csvReader.ReadAsync())
        {
            throw new InvalidOperationException("The selected CSV file is empty.");
        }

        csvReader.ReadHeader();

        var headerRecord = csvReader.HeaderRecord
            ?? throw new InvalidOperationException("The selected CSV file does not contain a header row.");

        _schemaValidator.ValidateOrThrow(headerRecord);

        var totalRows = 0;

        while (await csvReader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            totalRows++;

            try
            {
                var book = MapCurrentRow(csvReader);
                books.Add(book);
            }
            catch (Exception ex)
            {
                warnings.Add($"Row {csvReader.Context.Parser.Row} was skipped: {ex.Message}");
            }
        }

        return new CsvLoadResult(books, totalRows, warnings);
    }

    private BookRecord MapCurrentRow(CsvReader csvReader)
    {
        var instanceId = GetRequiredField(csvReader, _config.CsvColumns.RecordIdColumnName);
        var title = GetRequiredField(csvReader, _config.CsvColumns.TitleColumnName);
        var author = GetRequiredField(csvReader, _config.CsvColumns.AuthorColumnName);
        var rawNotes = GetRequiredField(csvReader, _config.CsvColumns.SummaryColumnName);
        var rawSubjects = GetRequiredField(csvReader, _config.CsvColumns.SubjectColumnName);

        var summary = _summaryExtractor.Extract(rawNotes);
        var subjectHeadings = ParseSubjectHeadings(rawSubjects);
        var additionalFields = ReadOptionalFields(csvReader);

        return new BookRecord(
            instanceId: instanceId,
            title: title,
            author: author,
            summary: summary,
            rawNotes: rawNotes,
            rawSubjects: rawSubjects,
            subjectHeadings: subjectHeadings,
            additionalFields: additionalFields);
    }

    private static string GetRequiredField(CsvReader csvReader, string columnName)
    {
        return csvReader.GetField(columnName)?.Trim() ?? string.Empty;
    }

    private static IReadOnlyList<SubjectHeading> ParseSubjectHeadings(string rawSubjects)
    {
        if (string.IsNullOrWhiteSpace(rawSubjects))
        {
            return Array.Empty<SubjectHeading>();
        }

        return rawSubjects
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(subject => !string.IsNullOrWhiteSpace(subject))
            .Select(subject => new SubjectHeading(subject))
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string> ReadOptionalFields(CsvReader csvReader)
    {
        var optionalFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var columnName in OptionalColumnNames)
        {
            if (csvReader.TryGetField<string>(columnName, out var value))
            {
                optionalFields[columnName] = value?.Trim() ?? string.Empty;
            }
        }

        return optionalFields;
    }
}
