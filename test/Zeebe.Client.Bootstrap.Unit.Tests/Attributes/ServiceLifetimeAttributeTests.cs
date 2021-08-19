using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Attributes
{
    public class ServiceLifetimeAttributeTests
    {
        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var serviceLifetime = ServiceLifetime.Singleton;            
            var attribute = new ServiceLifetimeAttribute(serviceLifetime);
            Assert.Equal(serviceLifetime, attribute.ServiceLifetime);
        }
    }
}
