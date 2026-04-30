using Heimdall.Application.Configuration;
using Heimdall.Application.Errors;
using Heimdall.Infrastructure.Csv;
using Heimdall.Infrastructure.Html;

namespace Heimdall.Tests.Csv;

public sealed class CsvErrorHandlingTests
{
    [Fact]
    public async Task ReadAsync_ThrowsFriendlyError_ForWrongFileType()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(tempFile, "not a csv");

        try
        {
            var reader = CreateReader();

            var exception = await Assert.ThrowsAsync<UserFriendlyException>(
                () => reader.ReadAsync(tempFile));

            Assert.Equal(HeimdallErrorCode.CsvWrongFileType, exception.ErrorCode);
            Assert.Equal("Wrong file type", exception.Title);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_ThrowsFriendlyError_ForEmptyCsv()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(tempFile, string.Empty);

        try
        {
            var reader = CreateReader();

            var exception = await Assert.ThrowsAsync<UserFriendlyException>(
                () => reader.ReadAsync(tempFile));

            Assert.Equal(HeimdallErrorCode.CsvEmpty, exception.ErrorCode);
            Assert.Contains("empty", exception.UserMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_ThrowsFriendlyError_ForMissingRequiredColumns()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        await File.WriteAllTextAsync(
            tempFile,
            "instances.title,instances.id" + Environment.NewLine +
            "Test Book,1");

        try
        {
            var reader = CreateReader();

            var exception = await Assert.ThrowsAsync<UserFriendlyException>(
                () => reader.ReadAsync(tempFile));

            Assert.Equal(HeimdallErrorCode.CsvMissingRequiredColumns, exception.ErrorCode);
            Assert.Contains("missing required", exception.Title, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static CsvBookRecordReader CreateReader()
    {
        var config = new HeimdallConfig();
        var validator = new CsvSchemaValidator(config);
        var summaryExtractor = new SummaryExtractor();

        return new CsvBookRecordReader(config, validator, summaryExtractor);
    }
}
