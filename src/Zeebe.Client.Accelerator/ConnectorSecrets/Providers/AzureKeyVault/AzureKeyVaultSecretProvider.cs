using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.AzureKeyVault
{
    public class AzureKeyVaultSecretProvider : ISecretProvider
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<AzureKeyVaultSecretProvider> _logger;

        public AzureKeyVaultSecretProvider(
            IOptions<AzureKeyVaultSecretProviderOptions> options,
            ILogger<AzureKeyVaultSecretProvider> logger,
            SecretClientOptions secretClientOptions = null)
        {
            _logger = logger;
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(config.VaultUri))
            {
                throw new ArgumentException("VaultUri must be configured for Azure Key Vault secret provider");
            }

            try
            {
                secretClientOptions ??= new SecretClientOptions();
                
                _secretClient = new SecretClient(
                    new Uri(config.VaultUri),
                    new DefaultAzureCredential(),
                    secretClientOptions);

                _logger.LogInformation("Initialized Azure Key Vault secret provider with vault URI: {VaultUri}",
                    config.VaultUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Key Vault secret provider");
                throw;
            }
        }

        public async Task<string> GetSecretAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Empty key provided to Azure Key Vault secret provider");
                return null;
            }

            try
            {
                var secret = await _secretClient.GetSecretAsync(key);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve secret {Key} from Azure Key Vault", key);
                return null;
            }
        }
    }
}