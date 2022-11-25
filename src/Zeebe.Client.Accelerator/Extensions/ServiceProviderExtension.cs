using Microsoft.Extensions.Hosting;
using System;

namespace Zeebe.Client.Accelerator.Extensions
{
    public static class ServiceProviderExtension
    {
        public static ZeebeResourceDeployer CreateZeebeDeployment(this IServiceProvider serviceProvider)
        {
            return new ZeebeResourceDeployer(serviceProvider);
        }

        public static ZeebeResourceDeployer CreateZeebeDeployment(this IHost host)
        {
            return new ZeebeResourceDeployer(host.Services);
        }
    }
}
