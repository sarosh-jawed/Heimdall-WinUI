namespace Heimdall.Application.Configuration;

public sealed class LoggingOptions
{
    public bool UseDocumentsFolder { get; init; } = true;
    public string LogFolderName { get; init; } = "Heimdall\\Logs";
    public string LogFileNameTemplate { get; init; } = "Heimdall-.log";
    public int RetainedFileCountLimit { get; init; } = 30;
}
