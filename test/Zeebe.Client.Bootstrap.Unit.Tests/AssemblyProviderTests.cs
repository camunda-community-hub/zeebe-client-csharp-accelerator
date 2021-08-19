using Xunit;
using System.Linq;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class AssemblyProviderTests
    {
        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var provider = new AssemblyProvider();
            Assert.NotNull(provider);
            Assert.Equal(57, provider.Assemblies.Count());
        }

        [Fact]
        public void AssembliesAreFilteredWhenConstructedWithAFilter() 
        {
            var provider = new AssemblyProvider(Meta.UNIT_TEST_PROJECT_NAME);
            Assert.Single(provider.Assemblies);

            provider = new AssemblyProvider(Meta.PROJECT_NAME);
            Assert.Equal(2, provider.Assemblies.Count());

            provider = new AssemblyProvider(Meta.PROJECT_NAME, "Client");
            Assert.Equal(3, provider.Assemblies.Count());
        }
    }
}
