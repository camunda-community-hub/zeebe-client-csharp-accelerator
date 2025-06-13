using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.Conjur;

public static class ConjurSecretProviderExtensions
{
    public static IServiceCollection AddConjurSecretProvider(
        this IServiceCollection services,
        IConfiguration zeebeConfigSection)
    {
        services.AddSecretProvider<ConjurSecretProvider>(
            zeebeConfigSection,
            (secretsSection, svc) => {
                svc.Configure<ConjurSecretProviderOptions>(secretsSection.GetSection("Conjur"));
                return svc;
            });
        return services;
    }
}