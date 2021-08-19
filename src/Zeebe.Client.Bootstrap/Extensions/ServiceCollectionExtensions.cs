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
            return services
                .AddZeebeClient(assembliesStartsWith)
                .Configure<ZeebeClientBootstrapOptions>(namedConfigurationSection)
                .AddHostedService<ZeebeHostedService>();
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, Action<ZeebeClientBootstrapOptions> configureOptions, params string[] assembliesStartsWith) 
        {
            return services
                .AddZeebeClient(assembliesStartsWith)
                .Configure(configureOptions)
                .AddHostedService<ZeebeHostedService>();
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, IConfiguration namedConfigurationSection, Action<ZeebeClientBootstrapOptions> configureOptions, params string[] assembliesStartsWith)
        {
            return services
                .AddZeebeClient(assembliesStartsWith)
                .Configure<ZeebeClientBootstrapOptions>(namedConfigurationSection)
                .PostConfigure(configureOptions)
                .AddHostedService<ZeebeHostedService>();
        }
        private static IServiceCollection AddZeebeClient(this IServiceCollection services, params string[] assembliesStartsWith)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));

            var assemblyprovider = new AssemblyProvider(assembliesStartsWith);

            return services                
                .AddZeebeBuilders()
                .AddSingleton(typeof(IAssemblyProvider), assemblyprovider)                
                .AddZeebeJobHandlers(assemblyprovider)
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

        private static IServiceCollection AddZeebeJobHandlers(this IServiceCollection services, IAssemblyProvider assemblyprovider) {
            var jobHandlerProvider = new JobHandlerProvider(assemblyprovider);
            services.AddSingleton(typeof(IJobHandlerProvider), jobHandlerProvider);

            foreach(var reference in jobHandlerProvider.JobHandlers)
            {
                if (IsAlreadyRegistered(services, reference.Handler.DeclaringType))
                    continue;

                services.Add(new ServiceDescriptor(reference.Handler.DeclaringType, reference.Handler.DeclaringType, reference.HandlerServiceLifetime));
            }

            return services;
        }

        private static bool IsAlreadyRegistered(IServiceCollection services, Type declaringType)
        {
            return services.Any(s => s.ServiceType == declaringType && s.ImplementationType == declaringType);
        }
    }
}