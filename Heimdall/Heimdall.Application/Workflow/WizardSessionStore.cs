using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Application.Workflow;

public sealed class WizardSessionStore
{
    public string? SourceCsvPath { get; set; }
    public string? OutputFolderPath { get; set; }
    public string? SubjectListFolderPath { get; set; }
    public string SubjectListMode { get; set; } = string.Empty;

    public CsvLoadResult? CsvLoadResult { get; set; }
    public SubjectListLoadResult? SubjectListLoadResult { get; set; }
    public CategoryMatchResult? CategoryMatchResult { get; set; }
    public EmailPreviewResult? PreviewResult { get; set; }

    public List<RemovedBookSelection> RemovedBookSelections { get; } = new();

    public void Reset()
    {
        SourceCsvPath = null;
        OutputFolderPath = null;
        SubjectListFolderPath = null;
        SubjectListMode = string.Empty;
        CsvLoadResult = null;
        SubjectListLoadResult = null;
        CategoryMatchResult = null;
        PreviewResult = null;
        RemovedBookSelections.Clear();
    }

    public void ResetPreviewState()
    {
        CategoryMatchResult = null;
        PreviewResult = null;
        RemovedBookSelections.Clear();
    }
}
