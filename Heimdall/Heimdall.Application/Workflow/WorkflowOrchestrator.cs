using Heimdall.Application.Contracts;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Application.Workflow;

public sealed class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private const string NotReadyMessage =
        "Workflow implementation starts in a later phase. The contract is intentionally registered now so startup infrastructure can be validated.";

    public Task<CsvLoadResult> LoadCsvAsync(
        string csvPath,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }

    public Task<SubjectListLoadResult> GenerateFreshSubjectListsAsync(
        string csvPath,
        string workingOutputFolder,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }

    public Task<SubjectListLoadResult> LoadExistingSubjectListsAsync(
        string subjectListFolder,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }

    public Task<EmailPreviewResult> BuildPreviewAsync(
        IReadOnlyList<CategoryKey> selectedCategories,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }

    public Task<EmailPreviewResult> RemoveBookFromPreviewAsync(
        CategoryKey category,
        string bookId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }

    public Task<HtmlExportResult> ExportAsync(
        string outputFolder,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(NotReadyMessage);
    }
}
