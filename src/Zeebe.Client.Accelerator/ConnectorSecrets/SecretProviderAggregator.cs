using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public class SecretProviderAggregator
{
    private readonly IList<ISecretProvider> _providers;
    private readonly ILogger<SecretProviderAggregator> _logger;
    private readonly SecretOptions _secretOptions;

    public SecretProviderAggregator(
        ILogger<SecretProviderAggregator> logger,
        IOptions<SecretOptions> options, 
        IEnumerable<ISecretProvider> providers)
    {
        _logger = logger;
        _secretOptions = options.Value;
        var orderedProviders = providers?.ToList() ?? new List<ISecretProvider>();
        if (!orderedProviders.Any())
        {
            var errorMessage = "No secret providers registered. Please ensure at least one provider is configured.";
            _logger.LogError(errorMessage);
        }
        _providers = orderedProviders.OrderBy(p => _secretOptions.Providers.IndexOf(p.GetType().Name)).ToList();
    }

    public async Task<string> GetSecretAsync(string key)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var value = await provider.GetSecretAsync(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {Key} from provider {Provider}", key,
                    provider.GetType().Name);
            }
        }

        _logger.LogWarning("Secret {Key} not found in any provider", key);
        return null;
    }
}