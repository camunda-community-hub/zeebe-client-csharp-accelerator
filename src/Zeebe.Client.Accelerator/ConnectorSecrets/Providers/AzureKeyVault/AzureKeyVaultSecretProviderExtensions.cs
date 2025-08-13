using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.AzureKeyVault;

public static class AzureKeyVaultSecretProviderExtensions
{
    public static IServiceCollection AddAzureKeyVaultSecretProvider(
        this IServiceCollection services,
        IConfiguration zeebeConfigSection)
    {
        services.AddSecretProvider<AzureKeyVaultSecretProvider>(
            zeebeConfigSection,
            (secretsSection, svc) => {
                svc.Configure<AzureKeyVaultSecretProviderOptions>(secretsSection.GetSection("AzureKeyVault"));
                return svc;
            });
        return services;
    }
}