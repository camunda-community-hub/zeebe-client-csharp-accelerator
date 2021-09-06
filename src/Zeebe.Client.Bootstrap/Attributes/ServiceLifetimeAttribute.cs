using System;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    public class ServiceLifetimeAttribute : AbstractJobHandlerAttribute
    {
        public ServiceLifetimeAttribute(ServiceLifetime serviceLifetime)
        {            
            this.ServiceLifetime = serviceLifetime;
        }

        public ServiceLifetime ServiceLifetime { get; }
    }
}