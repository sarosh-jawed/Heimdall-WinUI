using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface ICsvBookRecordReader
{
    Task<CsvLoadResult> ReadAsync(string csvPath, CancellationToken cancellationToken = default);
}
