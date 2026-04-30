namespace Heimdall.Application.Errors;

/// <summary>
/// Represents an expected recoverable Heimdall error that can be shown safely to staff.
/// Technical details stay in the inner exception and application logs.
/// </summary>
public sealed class UserFriendlyException : Exception
{
    public UserFriendlyException(
        HeimdallErrorCode errorCode,
        string title,
        string userMessage,
        string? recoveryHint = null,
        Exception? innerException = null)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        Title = title;
        UserMessage = userMessage;
        RecoveryHint = recoveryHint;
    }

    public HeimdallErrorCode ErrorCode { get; }

    public string Title { get; }

    public string UserMessage { get; }

    public string? RecoveryHint { get; }

    public string FullUserMessage =>
        string.IsNullOrWhiteSpace(RecoveryHint)
            ? UserMessage
            : $"{UserMessage} {RecoveryHint}";
}
