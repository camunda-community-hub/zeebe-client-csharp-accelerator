using System;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
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