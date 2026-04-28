using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Html;

public sealed class SummaryExtractorTests
{
    [Fact]
    public void Extract_ReturnsBlank_WhenNotesAreBlank()
    {
        var extractor = new SummaryExtractor();

        var summary = extractor.Extract("   ");

        Assert.Equal(string.Empty, summary);
    }

    [Fact]
    public void Extract_ReturnsPublicSummary_WhenJsonContainsSummaryNote()
    {
        var extractor = new SummaryExtractor();

        var rawNotes = """
        [
          {
            "note": "Summary: This is a public description for the book.",
            "staffOnly": false
          }
        ]
        """;

        var summary = extractor.Extract(rawNotes);

        Assert.Equal("This is a public description for the book.", summary);
    }

    [Fact]
    public void Extract_ReturnsBlank_WhenOnlyStaffNoteExists()
    {
        var extractor = new SummaryExtractor();

        var rawNotes = """
        [
          {
            "note": "Summary: Internal processing note.",
            "staffOnly": true
          }
        ]
        """;

        var summary = extractor.Extract(rawNotes);

        Assert.Equal(string.Empty, summary);
    }

    [Fact]
    public void Extract_DoesNotThrow_WhenNotesAreMalformed()
    {
        var extractor = new SummaryExtractor();

        var summary = extractor.Extract("{bad json");

        Assert.Equal(string.Empty, summary);
    }
}
