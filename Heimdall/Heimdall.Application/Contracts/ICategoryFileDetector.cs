using Heimdall.Domain.Results;

namespace Heimdall.Application.Contracts;

public interface ICategoryFileDetector
{
    CategoryFileDetectionResult Detect(string subjectListFolder);
}
