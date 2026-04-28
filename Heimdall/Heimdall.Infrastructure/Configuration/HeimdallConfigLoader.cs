using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Microsoft.Extensions.Configuration;

namespace Heimdall.Infrastructure.Configuration;

public sealed class HeimdallConfigLoader : IHeimdallConfigLoader
{
    public HeimdallConfig Load(string configPath)
    {
        if (string.IsNullOrWhiteSpace(configPath))
        {
            throw new ArgumentException("Configuration path cannot be blank.", nameof(configPath));
        }

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Heimdall configuration file was not found.", configPath);
        }

        var configDirectory = Path.GetDirectoryName(configPath)
            ?? throw new InvalidOperationException("Configuration directory could not be resolved.");

        var configFileName = Path.GetFileName(configPath);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configDirectory)
            .AddJsonFile(configFileName, optional: false, reloadOnChange: false)
            .Build();

        return configuration.GetSection("Heimdall").Get<HeimdallConfig>()
            ?? throw new InvalidOperationException("The Heimdall configuration section is missing or invalid.");
    }
}
