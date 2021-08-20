using Xunit;
using System.Linq;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class AssemblyProviderTests
    {
        [Fact]
        public void AllAssembliesAreFoundWhenCreated()
        {   
            var provider = new AssemblyProvider();
            Assert.NotNull(provider);
            Assert.Equal(57, provider.Assemblies.Count());
        }

        [Fact]
        public void AssembliesAreFilteredWhenCreatedWithAFilter() 
        {
            var provider = new AssemblyProvider(Meta.UNIT_TEST_PROJECT_NAME);
            Assert.Single(provider.Assemblies);

            provider = new AssemblyProvider(Meta.PROJECT_NAME);
            Assert.True(provider.Assemblies.All(a => a.FullName.StartsWith(Meta.PROJECT_NAME)));

            provider = new AssemblyProvider(Meta.PROJECT_NAME, "Client");
            Assert.True(provider.Assemblies.All(a => a.FullName.StartsWith(Meta.PROJECT_NAME) || a.FullName.StartsWith("Client")));
        }
    }
}
