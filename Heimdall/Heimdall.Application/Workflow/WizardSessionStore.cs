namespace Heimdall.Application.Workflow;

public sealed class WizardSessionStore
{
    public string? SourceCsvPath { get; set; }
    public string? OutputFolderPath { get; set; }
    public string? SubjectListFolderPath { get; set; }
    public string SubjectListMode { get; set; } = string.Empty;

    public void Reset()
    {
        SourceCsvPath = null;
        OutputFolderPath = null;
        SubjectListFolderPath = null;
        SubjectListMode = string.Empty;
    }
}
