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
            var expected = ServiceLifetime.Singleton;            
            var attribute = new ServiceLifetimeAttribute(expected);
            Assert.Equal(expected, attribute.ServiceLifetime);
        }
    }
}
