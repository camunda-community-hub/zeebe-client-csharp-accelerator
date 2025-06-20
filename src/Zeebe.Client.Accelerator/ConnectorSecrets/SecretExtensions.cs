using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public static class SecretExtensions
    {
        public static IServiceCollection AddConnectorSecrets(
            this IServiceCollection services, 
            IConfiguration zeebeConfigSection)
        {
            services.AddOptions<SecretOptions>()
                .Bind(zeebeConfigSection.GetSection(SecretOptions.Section));
            services.AddSingleton<SecretProviderAggregator>();
            services.AddSingleton<ISecretHandler,SecretHandler>();
            
            return services;
        }
        
        public static IServiceCollection AddSecretProvider<T>(
            this IServiceCollection services,
            IConfiguration zeebeConfigSection,
            Func<IConfiguration, IServiceCollection, IServiceCollection> configureProvider)
            where T : class, ISecretProvider
        {
            // Allow provider to register its own config/options from Zeebe section
            var secretsConfig = zeebeConfigSection.GetSection(SecretOptions.Section);
            configureProvider(secretsConfig, services);
            services.AddSingleton<ISecretProvider,T>();
            return services;
        }
    }