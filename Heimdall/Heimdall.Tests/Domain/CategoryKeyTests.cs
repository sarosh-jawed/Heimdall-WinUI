using Heimdall.Domain.ValueObjects;

namespace Heimdall.Tests.Domain;

public sealed class CategoryKeyTests
{
    [Fact]
    public void Constructor_NormalizesCategoryValue()
    {
        var key = new CategoryKey("  History    And   Art  ");

        Assert.Equal("history and art", key.Value);
    }

    [Fact]
    public void Constructor_Throws_WhenValueIsBlank()
    {
        Assert.Throws<ArgumentException>(() => new CategoryKey("   "));
    }
}
