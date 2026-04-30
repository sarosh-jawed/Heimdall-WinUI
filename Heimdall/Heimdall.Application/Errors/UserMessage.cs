namespace Heimdall.Application.Errors;

public sealed record UserMessage(
    string Title,
    string Message,
    HeimdallErrorCode ErrorCode);
