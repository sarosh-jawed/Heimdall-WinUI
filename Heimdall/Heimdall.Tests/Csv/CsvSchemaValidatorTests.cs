using Heimdall.Application.Configuration;
using Heimdall.Infrastructure.Csv;

namespace Heimdall.Tests.Csv;

public sealed class CsvSchemaValidatorTests
{
    [Fact]
    public void ValidateOrThrow_DoesNotThrow_WhenRequiredColumnsExist()
    {
        var validator = new CsvSchemaValidator(CreateConfig());

        var columns = new[]
        {
            "instances.title",
            "instances.instance_primary_contributor",
            "instances.notes",
            "instances.subjects",
            "instances.id"
        };

        validator.ValidateOrThrow(columns);
    }

    [Fact]
    public void ValidateOrThrow_ThrowsFriendlyError_WhenRequiredColumnIsMissing()
    {
        var validator = new CsvSchemaValidator(CreateConfig());

        var columns = new[]
        {
            "instances.title",
            "instances.instance_primary_contributor",
            "instances.notes",
            "instances.id"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => validator.ValidateOrThrow(columns));

        Assert.Contains("instances.subjects", exception.Message);
    }

    private static HeimdallConfig CreateConfig()
    {
        return new HeimdallConfig();
    }
}
