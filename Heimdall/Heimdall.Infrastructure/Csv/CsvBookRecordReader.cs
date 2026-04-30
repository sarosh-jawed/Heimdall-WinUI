using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Application.Errors;

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

        if (!Path.GetExtension(csvPath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvWrongFileType,
                "Wrong file type",
                "The selected input file is not a CSV file.",
                "Select the official FOLIO CSV file with a .csv extension.");
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
                var rowNumber = context.Context?.Parser?.Row ?? 0;
                warnings.Add($"Bad CSV data was found near row {rowNumber}.");
            },
            MissingFieldFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header?.Trim().ToLowerInvariant() ?? string.Empty
        };

        await using var stream = OpenCsvStream(csvPath);
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, csvConfiguration);

        if (!await csvReader.ReadAsync())
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvEmpty,
                "CSV is empty",
                "The selected CSV file is empty.",
                "Export a fresh official FOLIO CSV and try again.");
        }

        csvReader.ReadHeader();

        var headerRecord = csvReader.HeaderRecord;

        if (headerRecord is null || headerRecord.Length == 0)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvEmpty,
                "CSV header missing",
                "The selected CSV file does not contain a readable header row.",
                "Export a fresh official FOLIO CSV and try again.");
        }

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
                var rowNumber = GetCurrentRowNumber(csvReader);
                warnings.Add($"Row {rowNumber} was skipped: {ex.Message}");
            }
        }

        if (totalRows == 0)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvEmpty,
                "CSV has no book records",
                "The selected CSV has a header row but no book records.",
                "Export a fresh official FOLIO CSV that contains book rows.");
        }

        if (books.Count == 0)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvMalformed,
                "CSV has no usable book records",
                "Heimdall read the CSV, but every row was skipped because the records were malformed.",
                "Check the RunSummary/logs or export a fresh official FOLIO CSV.");
        }

        return new CsvLoadResult(books, totalRows, warnings);
    }

    private static FileStream OpenCsvStream(string csvPath)
    {
        try
        {
            return new FileStream(
                csvPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
        }
        catch (FileNotFoundException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvFileNotFound,
                "CSV file not found",
                "The selected FOLIO CSV file could not be found.",
                "Select the CSV file again.",
                ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvFileUnavailable,
                "CSV file cannot be accessed",
                "Heimdall does not have permission to read the selected CSV file.",
                "Move the file to a normal folder such as Documents or Desktop, then try again.",
                ex);
        }
        catch (IOException ex)
        {
            throw new UserFriendlyException(
                HeimdallErrorCode.CsvFileUnavailable,
                "CSV file unavailable",
                "Heimdall could not read the selected CSV file.",
                "Close the file if it is open in Excel or another program, then try again.",
                ex);
        }
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

    private static int GetCurrentRowNumber(CsvReader csvReader)
    {
        return csvReader.Context?.Parser?.Row ?? 0;
    }
}
