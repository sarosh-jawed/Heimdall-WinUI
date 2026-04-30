using Heimdall.Application.Contracts;

namespace Heimdall.Application.Errors;

public sealed class UserMessageService : IUserMessageService
{
    public UserMessage BuildMessage(
        Exception exception,
        string fallbackTitle,
        string fallbackMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);

        UserFriendlyException? friendlyException = FindUserFriendlyException(exception);

        if (friendlyException is not null)
        {
            return new UserMessage(
                friendlyException.Title,
                friendlyException.FullUserMessage,
                friendlyException.ErrorCode);
        }

        return exception switch
        {
            OperationCanceledException => new UserMessage(
                "Operation canceled",
                "The operation was canceled. No files were changed, and you can try again when ready.",
                HeimdallErrorCode.OperationCanceled),

            UnauthorizedAccessException => new UserMessage(
                "Permission denied",
                "Heimdall does not have permission to access the selected file or folder. Choose a different location or check folder permissions.",
                HeimdallErrorCode.OutputFolderNotWritable),

            FileNotFoundException => new UserMessage(
                "File not found",
                "The selected file could not be found. It may have been moved, deleted, or disconnected. Select the file again.",
                HeimdallErrorCode.CsvFileNotFound),

            DirectoryNotFoundException => new UserMessage(
                "Folder not found",
                "The selected folder could not be found. It may have been moved, deleted, or disconnected. Select the folder again.",
                HeimdallErrorCode.SubjectListFolderNotFound),

            IOException => new UserMessage(
                "File or folder unavailable",
                "Heimdall could not access the selected file or folder. Close the file if it is open in another program, then try again.",
                HeimdallErrorCode.CsvFileUnavailable),

            _ => new UserMessage(
                fallbackTitle,
                fallbackMessage,
                HeimdallErrorCode.Unknown)
        };
    }

    private static UserFriendlyException? FindUserFriendlyException(Exception exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is UserFriendlyException friendlyException)
            {
                return friendlyException;
            }

            current = current.InnerException;
        }

        return null;
    }
}
