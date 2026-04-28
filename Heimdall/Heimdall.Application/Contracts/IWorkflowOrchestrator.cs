using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;

namespace Heimdall.Application.Contracts;

public interface IWorkflowOrchestrator
{
    Task<CsvLoadResult> LoadCsvAsync(
        string csvPath,
        CancellationToken cancellationToken = default);

    Task<SubjectListLoadResult> GenerateFreshSubjectListsAsync(
        string csvPath,
        string workingOutputFolder,
        CancellationToken cancellationToken = default);

    Task<SubjectListLoadResult> LoadExistingSubjectListsAsync(
        string subjectListFolder,
        CancellationToken cancellationToken = default);

    Task<EmailPreviewResult> BuildPreviewAsync(
        IReadOnlyList<CategoryKey> selectedCategories,
        CancellationToken cancellationToken = default);

    Task<EmailPreviewResult> RemoveBookFromPreviewAsync(
        CategoryKey category,
        string bookId,
        CancellationToken cancellationToken = default);

    Task<HtmlExportResult> ExportAsync(
        string outputFolder,
        CancellationToken cancellationToken = default);
}
