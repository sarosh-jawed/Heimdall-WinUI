using Heimdall.Domain.ValueObjects;

namespace Heimdall.Tests.Domain;

public sealed class OutputFileNameTests
{
    [Fact]
    public void Constructor_AcceptsValidFileName()
    {
        var fileName = new OutputFileName("HistoryNewBooks2026-04-28.html");

        Assert.Equal("HistoryNewBooks2026-04-28.html", fileName.Value);
    }

    [Fact]
    public void Constructor_RejectsInvalidFileNameCharacters()
    {
        Assert.Throws<ArgumentException>(() => new OutputFileName("bad:file.html"));
    }
}
