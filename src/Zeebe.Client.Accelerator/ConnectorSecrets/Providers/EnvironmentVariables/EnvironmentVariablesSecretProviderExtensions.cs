using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

public static class EnvironmentVariablesSecretProviderExtensions
{
    public static IServiceCollection AddEnvironmentSecretProvider(
        this IServiceCollection services,
        IConfiguration zeebeConfigSection)
    {
        services.AddSecretProvider<EnvironmentVariablesSecretProvider>(
            zeebeConfigSection,
            (secretsSection, svc) => {
                svc.Configure<EnvironmentVariablesSecretProviderOptions>(secretsSection.GetSection("EnvironmentVariables"));
                return svc;
            });
        return services;
    }
}