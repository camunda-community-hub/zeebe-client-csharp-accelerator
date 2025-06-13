using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

namespace Zeebe.Client.Accelerator.Unit.Tests.ConnectorSecrets;

public class SecretHandlerTests
{
    private static readonly string _secretKey = $"SECRET_{Guid.NewGuid():N}";
    private readonly string _envKey =  $"TEST_{_secretKey}";
    private readonly string _testValue = $"test-value-{Guid.NewGuid():N}";
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithEnvironmentVariablesProvider_ShouldResolveSecrets()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_envKey, _testValue);
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Zeebe:ConnectorSecrets:Providers:0"] = "EnvironmentVariablesSecretProvider",
                    ["Zeebe:ConnectorSecrets:EnvironmentVariables:Prefix"] = "TEST_"
                })
                .AddEnvironmentVariables() 
                .Build();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddConnectorSecrets(configuration.GetSection("Zeebe"));
            services.AddEnvironmentSecretProvider(configuration.GetSection("Zeebe"));
            var serviceProvider = services.BuildServiceProvider();
            var secretHandler = serviceProvider.GetRequiredService<ISecretHandler>();

            // Act
            var result = await secretHandler.ReplaceSecretsAsync($"The value is {{{{ secrets.{_secretKey} }}}}");

            // Assert
            Assert.Equal($"The value is {_testValue}", result);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable(_envKey, null);
        }
    }
}