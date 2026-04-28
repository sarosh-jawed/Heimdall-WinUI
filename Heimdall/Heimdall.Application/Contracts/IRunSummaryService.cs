using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface IRunSummaryService
{
    string RenderText(RunSummary runSummary);
}
