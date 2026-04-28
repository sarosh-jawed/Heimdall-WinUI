using System.Text;
using Heimdall.Application.Configuration;
using Heimdall.Infrastructure.Csv;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Csv;

public sealed class CsvBookRecordReaderTests
{
    [Fact]
    public async Task ReadAsync_LoadsThirtyFourRecords()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 34));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);

            Assert.Equal(34, result.TotalRows);
            Assert.Equal(34, result.Books.Count);
            Assert.Empty(result.Warnings);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_ExtractsTitleAuthorAndInstanceId()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 1));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Equal("instance-001", book.InstanceId);
            Assert.Equal("The First Test Book", book.Title);
            Assert.Equal("Doe, Jane", book.Author);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_PreservesRawNotesAndRawSubjects()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 1));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Contains("Summary:", book.RawNotes);
            Assert.Equal("History; Education", book.RawSubjects);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_ExtractsTemporarySummaryFromNotes()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 1));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Equal("This is the public summary for the first test book.", book.Summary);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_ParsesSubjectHeadings()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 1));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Equal(2, book.SubjectHeadings.Count);
            Assert.Contains(book.SubjectHeadings, subject => subject.NormalizedValue == "history");
            Assert.Contains(book.SubjectHeadings, subject => subject.NormalizedValue == "education");
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_MapsOptionalDebugFields()
    {
        var csvPath = WriteTempCsv(BuildFolioshapedCsv(rowCount: 1));

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Equal("hrid-001", book.AdditionalFields["instances.hrid"]);
            Assert.Equal("barcode-001", book.AdditionalFields["items.barcode"]);
            Assert.Equal("QA 001", book.AdditionalFields["items.effective_call_number"]);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    [Fact]
    public async Task ReadAsync_DoesNotCrash_WhenOptionalTextFieldsAreEmpty()
    {
        var csv = string.Join(
            Environment.NewLine,
            HeaderLine,
            string.Join(",", new[]
            {
                Escape("instance-empty"),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape(""),
                Escape("")
            }));

        var csvPath = WriteTempCsv(csv);

        try
        {
            var reader = CreateReader();

            var result = await reader.ReadAsync(csvPath);
            var book = result.Books.Single();

            Assert.Equal("instance-empty", book.InstanceId);
            Assert.Equal(string.Empty, book.Title);
            Assert.Equal(string.Empty, book.Author);
            Assert.Equal(string.Empty, book.Summary);
            Assert.False(book.HasSubjects);
        }
        finally
        {
            File.Delete(csvPath);
        }
    }

    private static CsvBookRecordReader CreateReader()
    {
        var config = new HeimdallConfig();
        var schemaValidator = new CsvSchemaValidator(config);
        var summaryExtractor = new SummaryExtractor();

        return new CsvBookRecordReader(config, schemaValidator, summaryExtractor);
    }

    private const string HeaderLine =
        "instances.id,instances.title,instances.instance_primary_contributor,instances.notes,instances.subjects,instances.hrid,items.barcode,items.effective_call_number,effective_location.name,mtypes.name,instances.publication";

    private static string BuildFolioshapedCsv(int rowCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine(HeaderLine);

        for (var index = 1; index <= rowCount; index++)
        {
            var id = $"instance-{index:000}";
            var title = index == 1 ? "The First Test Book" : $"Generated Test Book {index}";
            var author = index == 1 ? "Doe, Jane" : $"Author {index}";
            var notes = index == 1
                ? "[{\"note\":\"Summary: This is the public summary for the first test book.\",\"staffOnly\":false}]"
                : string.Empty;
            var subjects = index == 1 ? "History; Education" : "History";

            var fields = new[]
            {
                id,
                title,
                author,
                notes,
                subjects,
                $"hrid-{index:000}",
                $"barcode-{index:000}",
                $"QA {index:000}",
                "Library Display",
                "Book",
                "2026"
            };

            builder.AppendLine(string.Join(",", fields.Select(Escape)));
        }

        return builder.ToString();
    }

    private static string WriteTempCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"heimdall-folio-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    private static string Escape(string value)
    {
        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value}\"";
        }

        return value;
    }
}
