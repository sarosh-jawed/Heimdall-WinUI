using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Tests.Domain;

public sealed class RunSummaryTests
{
    [Fact]
    public void RunSummary_CanBeConstructed()
    {
        var startedAt = DateTimeOffset.Now;
        var endedAt = startedAt.AddMinutes(2);

        var summary = new RunSummary
        {
            RunStartedAt = startedAt,
            RunEndedAt = endedAt,
            SourceCsvPath = "books.csv",
            SubjectListMode = "ExistingFolder",
            SelectedCategories = new[] { new CategoryKey("History") },
            TotalRecordsRead = 34,
            MatchedRecordCounts = new Dictionary<CategoryKey, int>
            {
                [new CategoryKey("History")] = 21
            },
            CannotSortCount = 8,
            GeneratedFiles = new[]
            {
                new OutputFileName("HistoryNewBooks2026-04-28.html")
            }
        };

        Assert.Equal("books.csv", summary.SourceCsvPath);
        Assert.Equal(34, summary.TotalRecordsRead);
        Assert.Equal(8, summary.CannotSortCount);
        Assert.NotNull(summary.Duration);
    }
}
