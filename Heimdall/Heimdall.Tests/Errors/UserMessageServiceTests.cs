using Heimdall.Application.Errors;

namespace Heimdall.Tests.Errors;

public sealed class UserMessageServiceTests
{
    [Fact]
    public void BuildMessage_UsesUserFriendlyExceptionMessage()
    {
        var service = new UserMessageService();

        var exception = new UserFriendlyException(
            HeimdallErrorCode.CsvWrongFileType,
            "Wrong file type",
            "The selected input file is not a CSV file.",
            "Select a .csv file.");

        var message = service.BuildMessage(
            exception,
            "Fallback title",
            "Fallback message");

        Assert.Equal("Wrong file type", message.Title);
        Assert.Contains("not a CSV", message.Message);
        Assert.Contains("Select a .csv file", message.Message);
        Assert.Equal(HeimdallErrorCode.CsvWrongFileType, message.ErrorCode);
    }

    [Fact]
    public void BuildMessage_MapsOperationCanceledException()
    {
        var service = new UserMessageService();

        var message = service.BuildMessage(
            new OperationCanceledException(),
            "Fallback title",
            "Fallback message");

        Assert.Equal("Operation canceled", message.Title);
        Assert.Contains("canceled", message.Message);
        Assert.Equal(HeimdallErrorCode.OperationCanceled, message.ErrorCode);
    }

    [Fact]
    public void BuildMessage_HidesUnexpectedTechnicalExceptionMessage()
    {
        var service = new UserMessageService();

        var message = service.BuildMessage(
            new InvalidOperationException("Sensitive technical details"),
            "Something went wrong",
            "Heimdall could not complete the operation.");

        Assert.Equal("Something went wrong", message.Title);
        Assert.Equal("Heimdall could not complete the operation.", message.Message);
        Assert.Equal(HeimdallErrorCode.Unknown, message.ErrorCode);
    }
}
