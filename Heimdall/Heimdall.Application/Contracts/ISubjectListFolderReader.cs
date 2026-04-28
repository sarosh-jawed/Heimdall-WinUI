using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface ISubjectListFolderReader
{
    Task<SubjectListLoadResult> ReadAsync(
        string subjectListFolder,
        CancellationToken cancellationToken = default);
}
