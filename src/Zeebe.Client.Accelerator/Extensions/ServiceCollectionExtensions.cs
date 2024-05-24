using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Api.Builder;
using Microsoft.Extensions.Options;
using Zeebe.Client.Accelerator.Options;
using System.Reflection;

namespace Zeebe.Client.Accelerator.Extensions
{
    public static class ServiceCollectionExtensions 
    {
        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, IConfiguration namedConfigurationSection, params Assembly[] assemblies)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (namedConfigurationSection is null)
            {
                throw new ArgumentNullException(nameof(namedConfigurationSection));
            }

            services
                .BootstrapZeebe(assemblies)
                .AddOptions<ZeebeClientAcceleratorOptions>()
                .Bind(namedConfigurationSection)
                .Validate(ValidateZeebeClientBootstrapOptions);

            return services;
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, Action<ZeebeClientAcceleratorOptions> configureOptions, params Assembly[] assemblies) 
        {
            if (configureOptions is null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services
                .BootstrapZeebe(assemblies)
                .AddOptions<ZeebeClientAcceleratorOptions>()
                .Configure(configureOptions)
                .Validate(ValidateZeebeClientBootstrapOptions);

            return services;
        }

        public static IServiceCollection BootstrapZeebe(this IServiceCollection services, IConfiguration namedConfigurationSection, Action<ZeebeClientAcceleratorOptions> postConfigureOptions, params Assembly[] assemblies)
        {
            if (namedConfigurationSection is null)
            {
                throw new ArgumentNullException(nameof(namedConfigurationSection));
            }

            if (postConfigureOptions is null)
            {
                throw new ArgumentNullException(nameof(postConfigureOptions));
            }

            services
                .BootstrapZeebe(assemblies)
                .AddOptions<ZeebeClientAcceleratorOptions>()
                .Bind(namedConfigurationSection)
                .PostConfigure(postConfigureOptions)
                .Validate(ValidateZeebeClientBootstrapOptions);

            return services;
        }

        private static bool ValidateZeebeClientBootstrapOptions(ZeebeClientAcceleratorOptions options)
        {
            var validator = new ZeebeClientAcceleratorOptionsValidator();
            validator.Validate(options);
            return true;
        }

        private static IServiceCollection BootstrapZeebe(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null) 
                throw new ArgumentNullException(nameof(services));

            return services
                .AddScoped(typeof(IBootstrapJobHandler), typeof(ZeebeJobHandler))
                .AddSingleton(typeof(IZeebeVariablesSerializer), typeof(ZeebeVariablesSerializer))
                .AddSingleton(typeof(IZeebeVariablesDeserializer), typeof(ZeebeVariablesDeserializer))
                .AddZeebeJobHandlers(assemblies)
                .AddZeebeClient()
                .AddHostedService<ZeebeHostedService>();
        }

        private static IServiceCollection AddZeebeJobHandlers(this IServiceCollection services, Assembly[] assemblies) {
            var jobHandlerProvider = new JobHandlerInfoProvider(assemblies);
            services.AddSingleton(typeof(IJobHandlerInfoProvider), jobHandlerProvider);

            foreach(var reference in jobHandlerProvider.JobHandlerInfoCollection)
            {
                if (IsAlreadyRegistered(services, reference.Handler.ReflectedType))
                    continue;

                services.Add(new ServiceDescriptor(reference.Handler.ReflectedType, reference.Handler.ReflectedType, reference.HandlerServiceLifetime));
            }

            return services;
        }

        private static IServiceCollection AddZeebeClient(this IServiceCollection services)
        {
            return services
                .AddZeebeBuilders()
                .AddScoped(sp => {
                    var options = sp.GetRequiredService<IOptions<ZeebeClientAcceleratorOptions>>();
                    var builder = sp.GetRequiredService<IZeebeClientBuilder>();
                    var tokenSupplier = sp.GetService<IAccessTokenSupplier>();
                    var loggerFactory = sp.GetService<ILoggerFactory>();
                    StateBuilderExtensions.Configure(sp.GetRequiredService<IZeebeVariablesSerializer>());

                    if(loggerFactory != null)
                        builder = builder.UseLoggerFactory(loggerFactory);

                    return builder
                        .Build(options.Value.Client, tokenSupplier);
                });
        }

        private static bool IsAlreadyRegistered(IServiceCollection services, Type reflectedType)
        {
            return services.Any(s => s.ServiceType == reflectedType && s.ImplementationType == reflectedType);
        }

    }
}