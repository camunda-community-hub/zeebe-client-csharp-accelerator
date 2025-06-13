using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

public class EnvironmentVariablesSecretProvider : ISecretProvider
{
    private readonly EnvironmentVariablesSecretProviderOptions _options;
    private readonly ILogger<EnvironmentVariablesSecretProvider> _logger;

    public EnvironmentVariablesSecretProvider(
        ILogger<EnvironmentVariablesSecretProvider> logger, 
        IOptions<EnvironmentVariablesSecretProviderOptions> options)
    {
        _logger = logger;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string> GetSecretAsync(string key)
    {
        string fullKey = string.IsNullOrEmpty(_options.Prefix) ? key : $"{_options.Prefix}{key}";
        string value = Environment.GetEnvironmentVariable(fullKey);
            
        if (string.IsNullOrEmpty(value))
        {
            _logger.LogDebug("Secret not found in environment variables: {Key}", fullKey);
            return null;
        }
            
        return await Task.FromResult(value);
    }
}