using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Api.Builder;
using Microsoft.Extensions.Options;
using Zeebe.Client.Bootstrap.Options;

namespace Zeebe.Client.Bootstrap.Extensions
{
    public static class ServiceCollectionExtensions 
    {
        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, IConfiguration namedConfigurationSection, params string[] assembliesStartsWith)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (namedConfigurationSection is null)
            {
                throw new ArgumentNullException(nameof(namedConfigurationSection));
            }

            return services
                .BootstrapZeebe(assembliesStartsWith)
                .Configure<ZeebeClientBootstrapOptions>(namedConfigurationSection);
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, Action<ZeebeClientBootstrapOptions> configureOptions, params string[] assembliesStartsWith) 
        {
            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            return services
                .BootstrapZeebe(assembliesStartsWith)
                .Configure(configureOptions);
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, IConfiguration namedConfigurationSection, Action<ZeebeClientBootstrapOptions> postConfigureOptions, params string[] assembliesStartsWith)
        {
            if (namedConfigurationSection is null)
            {
                throw new ArgumentNullException(nameof(namedConfigurationSection));
            }

            if (postConfigureOptions is null)
            {
                throw new ArgumentNullException(nameof(postConfigureOptions));
            }

            return services                
                .BootstrapZeebe(assembliesStartsWith)
                .Configure<ZeebeClientBootstrapOptions>(namedConfigurationSection)
                .PostConfigure(postConfigureOptions);
        }

        private static IServiceCollection BootstrapZeebe(this IServiceCollection services, params string[] assembliesStartsWith)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));

            var assemblyprovider = new AssemblyProvider(assembliesStartsWith);

            return services
                .AddSingleton(typeof(IAssemblyProvider), assemblyprovider)
                .AddSingleton(typeof(IBootstrapJobHandler), typeof(BootstrapJobHandler))
                .AddSingleton(typeof(IZeebeVariablesSerializer), typeof(ZeebeVariablesSerializer))
                .AddZeebeJobHandlers(assemblyprovider)
                .AddZeebeClient()
                .AddHostedService<ZeebeHostedService>();
        }

        private static IServiceCollection AddZeebeJobHandlers(this IServiceCollection services, IAssemblyProvider assemblyprovider) {
            var jobHandlerProvider = new JobHandlerInfoProvider(assemblyprovider);
            services.AddSingleton(typeof(IJobHandlerInfoProvider), jobHandlerProvider);

            foreach(var reference in jobHandlerProvider.JobHandlerInfoCollection)
            {
                if (IsAlreadyRegistered(services, reference.Handler.DeclaringType))
                    continue;

                services.Add(new ServiceDescriptor(reference.Handler.DeclaringType, reference.Handler.DeclaringType, reference.HandlerServiceLifetime));
            }

            return services;
        }

        private static IServiceCollection AddZeebeClient(this IServiceCollection services)
        {
            return services                
                .AddZeebeBuilders()
                .AddScoped(sp => {
                    var options = sp.GetRequiredService<IOptions<ZeebeClientBootstrapOptions>>();
                    var builder = sp.GetRequiredService<IZeebeClientBuilder>();
                    var loggerFactory = sp.GetService<ILoggerFactory>();
                    
                    if(loggerFactory != null)
                        builder = builder.UseLoggerFactory(loggerFactory);
                        
                    return builder
                        .Build(options.Value);                        
                });
        }

        private static bool IsAlreadyRegistered(IServiceCollection services, Type declaringType)
        {
            return services.Any(s => s.ServiceType == declaringType && s.ImplementationType == declaringType);
        }
    }
}