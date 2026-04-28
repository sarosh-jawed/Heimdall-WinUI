using Heimdall.Application.Configuration;

namespace Heimdall.Application.Contracts;

public interface IHeimdallConfigLoader
{
    HeimdallConfig Load(string configPath);
}
