using Heimdall.Domain.Models;

namespace Heimdall.Tests.Domain;

public sealed class BookRecordTests
{
    [Fact]
    public void Constructor_Throws_WhenTitleAndInstanceIdAreBlank()
    {
        Assert.Throws<ArgumentException>(() => new BookRecord(null, null));
    }

    [Fact]
    public void Constructor_AllowsRecord_WhenTitleIsPresent()
    {
        var record = new BookRecord(
            instanceId: null,
            title: "Example Book");

        Assert.Equal("Example Book", record.Title);
    }

    [Fact]
    public void Constructor_AllowsRecord_WhenInstanceIdIsPresent()
    {
        var record = new BookRecord(
            instanceId: "folio-123",
            title: null);

        Assert.Equal("folio-123", record.InstanceId);
    }
}
