using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public class SecretHandler : ISecretHandler
{
    private readonly SecretProviderAggregator _secretProviderAggregator;
    private readonly ILogger<SecretHandler> _logger;

    public SecretHandler(SecretProviderAggregator secretProviderAggregator, ILogger<SecretHandler> logger)
    {
        _secretProviderAggregator = secretProviderAggregator ?? throw new ArgumentNullException(nameof(secretProviderAggregator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ReplaceSecretsAsync(string input)
    {
        return await SecretUtil.ReplaceSecretsAsync(input, async name =>
        {
            var secret = await _secretProviderAggregator.GetSecretAsync(name);
            if (secret == null)
            {
                _logger.LogWarning("Secret with name '{SecretName}' is not available", name);
                throw new ConnectorInputException($"Secret with name '{name}' is not available");
            }
            return secret;
        });
    }
}