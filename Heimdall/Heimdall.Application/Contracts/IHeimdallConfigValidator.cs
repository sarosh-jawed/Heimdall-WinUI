using Heimdall.Application.Configuration;

namespace Heimdall.Application.Contracts;

public interface IHeimdallConfigValidator
{
    void ValidateAndThrow(HeimdallConfig config);
}
