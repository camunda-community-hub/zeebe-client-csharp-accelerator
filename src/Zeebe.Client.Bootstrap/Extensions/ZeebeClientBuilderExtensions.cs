using System;
using Zeebe.Client.Api.Builder;
using Zeebe.Client.Bootstrap.Options;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Extensions
{
     public static class ZeebeClientBuilderExtensions 
     {
         public static IZeebeClient Build(this IZeebeClientBuilder builder, ZeebeClientBootstrapOptions options) {
             return builder
                .BuildTransportBuilder(options.Client)
                .BuildFinalStep(options.Client)
                .BuildClient(options.Client);
         }

        private static IZeebeClientTransportBuilder BuildTransportBuilder(this IZeebeClientBuilder builder, ClientOptions options) {
            if(String.IsNullOrEmpty(options.GatewayAddress))
                throw new ArgumentNullException(nameof(options.GatewayAddress));

            return builder.UseGatewayAddress(options.GatewayAddress);
        }

        private static IZeebeClientFinalBuildStep BuildFinalStep(this IZeebeClientTransportBuilder builder, ClientOptions options) {
            if(options.TransportEncryption == null)
                return builder.UsePlainText();

            if(!String.IsNullOrEmpty(options.TransportEncryption.RootCertificatePath))
                return builder.UseTransportEncryption(options.TransportEncryption.RootCertificatePath);

            if(!string.IsNullOrEmpty(options.TransportEncryption.AccessToken))
                return builder.UseTransportEncryption().UseAccessToken(options.TransportEncryption.AccessToken);

            if(options.TransportEncryption.AccessTokenSupplier != null)
                return builder.UseTransportEncryption().UseAccessTokenSupplier(options.TransportEncryption.AccessTokenSupplier);

            throw new NotImplementedException($"{nameof(options.TransportEncryption)} is instantiated but none of it's properties have valid values.");
        }

        private static IZeebeClient BuildClient(this IZeebeClientFinalBuildStep builder, ClientOptions options) {
            if(options.KeepAliveInMilliSeconds.HasValue)
                builder = builder.UseKeepAlive(options.KeepAlive);

            if(options.RetrySleepDurationProvider != null)
                builder = builder.UseRetrySleepDurationProvider(options.RetrySleepDurationProvider);

            return builder.Build();
        }
     }
}