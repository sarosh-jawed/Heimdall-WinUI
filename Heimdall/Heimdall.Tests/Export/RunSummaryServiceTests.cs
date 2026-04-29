using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;
using Heimdall.Infrastructure.Export;

namespace Heimdall.Tests.Export;

public sealed class RunSummaryServiceTests
{
    [Fact]
    public void RenderText_IncludesAllRequiredSections()
    {
        RunSummaryService service = new();

        var artKey = new CategoryKey("Art");
        var historyKey = new CategoryKey("History");

        var summary = new RunSummary
        {
            RunStartedAt = new DateTimeOffset(2026, 4, 29, 10, 0, 0, TimeSpan.Zero),
            RunEndedAt = new DateTimeOffset(2026, 4, 29, 10, 1, 0, TimeSpan.Zero),
            SourceCsvPath = @"C:\Input\2nd Floor Display Books.csv",
            SubjectListMode = "ExistingFolder",
            SubjectListFolderPath = @"C:\Bragi\Output",
            SelectedCategories = new[] { artKey, historyKey },
            TotalRecordsRead = 34,
            MatchedRecordCounts = new Dictionary<CategoryKey, int>
            {
                [artKey] = 5,
                [historyKey] = 8
            },
            RemovedRecordCounts = new Dictionary<CategoryKey, int>
            {
                [artKey] = 1,
                [historyKey] = 0
            },
            CannotSortCount = 3,
            GeneratedFiles = new[]
            {
                new OutputFileName("ArtNewBooks2026-04-29.html"),
                new OutputFileName("HistoryNewBooks2026-04-29.html"),
                new OutputFileName("CannotSortBooks2026-04-29.html"),
                new OutputFileName("RunSummary2026-04-29.txt")
            },
            Warnings = new[] { "Sample warning." }
        };

        string text = service.RenderText(summary);

        Assert.Contains("Heimdall Run Summary", text);
        Assert.Contains("Run Timing:", text);
        Assert.Contains("Input:", text);
        Assert.Contains("Record Counts:", text);
        Assert.Contains("Selected Categories:", text);
        Assert.Contains("Matched Records by Category:", text);
        Assert.Contains("Removed Records by Category:", text);
        Assert.Contains("Generated Files:", text);
        Assert.Contains("Warnings:", text);

        Assert.Contains(@"C:\Input\2nd Floor Display Books.csv", text);
        Assert.Contains("ExistingFolder", text);
        Assert.Contains(@"C:\Bragi\Output", text);
        Assert.Contains("Total Records Read: 34", text);
        Assert.Contains("Cannot Sort Count: 3", text);
        Assert.Contains("art: 5", text);
        Assert.Contains("history: 8", text);
        Assert.Contains("art: 1", text);
        Assert.Contains("RunSummary2026-04-29.txt", text);
        Assert.Contains("Sample warning.", text);
    }

    [Fact]
    public void RenderText_ShowsNoneForEmptyLists()
    {
        RunSummaryService service = new();

        var summary = new RunSummary
        {
            RunStartedAt = new DateTimeOffset(2026, 4, 29, 10, 0, 0, TimeSpan.Zero),
            RunEndedAt = new DateTimeOffset(2026, 4, 29, 10, 1, 0, TimeSpan.Zero),
            TotalRecordsRead = 0
        };

        string text = service.RenderText(summary);

        Assert.Contains("Selected Categories:", text);
        Assert.Contains("Matched Records by Category:", text);
        Assert.Contains("Removed Records by Category:", text);
        Assert.Contains("Generated Files:", text);
        Assert.Contains("Warnings:", text);
        Assert.Contains("- None.", text);
    }
}
