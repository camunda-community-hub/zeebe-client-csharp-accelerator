using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes
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
