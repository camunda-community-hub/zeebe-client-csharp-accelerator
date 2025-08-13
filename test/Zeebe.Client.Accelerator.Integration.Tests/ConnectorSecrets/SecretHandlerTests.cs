using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.AzureKeyVault;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

namespace Zeebe.Client.Accelerator.Integration.Tests.ConnectorSecrets;

public class SecretHandlerTests : IClassFixture<ConnectorSecretsFixture>
{
    
    private readonly ConnectorSecretsFixture _fixture;
    private readonly ISecretHandler _sut;
   
    public SecretHandlerTests(ConnectorSecretsFixture fixture)
    {
        _fixture = fixture;
        _fixture.ConfigureServices();
        _sut = _fixture.ServiceProvider.GetRequiredService<ISecretHandler>();
    }

    [Fact]
    public async Task ReplaceSecretsAsync_WithEnvironmentVariablesProvider_ShouldResolveSecrets()
    {
        // Arrange
        var secretKey = $"SECRET-{Guid.NewGuid():N}";
        var testValue = $"test-value-{Guid.NewGuid():N}";
        await _fixture.SeedEnvironmentVariableSecret(secretKey, testValue);

        // Act
        var result = await _sut.ReplaceSecretsAsync($"The value is {{{{ secrets.{secretKey} }}}}");

        // Assert
        Assert.Equal($"The value is {testValue}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithSimplePattern_ShouldResolveSecrets()
    {
        // Arrange
        var secretKey = $"SECRET-{Guid.NewGuid():N}";
        var testValue = $"test-value-{Guid.NewGuid():N}";
        await _fixture.SeedEnvironmentVariableSecret(secretKey, testValue);

        // Act
        var result = await _sut.ReplaceSecretsAsync($"The value is secrets.{secretKey}");

        // Assert
        Assert.Equal($"The value is {testValue}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithMultipleSecrets_ShouldResolveSecrets()
    {
        // Arrange
        var secretKey1 = $"SECRET-{Guid.NewGuid():N}";
        var testValue1 = $"test-value-{Guid.NewGuid():N}";
        var secretKey2 = $"SECRET-{Guid.NewGuid():N}";
        var testValue2 = $"test-value-{Guid.NewGuid():N}";
        var secretKey3 = $"SECRET-{Guid.NewGuid():N}";
        var testValue3 = $"test-value-{Guid.NewGuid():N}";
        await _fixture.SeedEnvironmentVariableSecret(secretKey1, testValue1);
        await _fixture.SeedEnvironmentVariableSecret(secretKey2, testValue2);
        await _fixture.SeedEnvironmentVariableSecret(secretKey3, testValue3);

        // Act
        var result = await _sut.ReplaceSecretsAsync(
            $"First: {{{{ secrets.{secretKey1} }}}}, Second: secrets.{secretKey2}, Third: {{{{ secrets.{secretKey3} }}}}");

        // Assert
        Assert.Equal($"First: {testValue1}, Second: {testValue2}, Third: {testValue3}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithNoRegisteredProviders_ReturnsInput()
    {
        // Arrange
        _fixture.ConfigureServices(includeSecretProvider: false);
        var sampleText = $"{Guid.NewGuid():N}";
        
        // Act
        var result = await _sut.ReplaceSecretsAsync($"The value is {sampleText}");

        // Act & Assert
        Assert.Equal($"The value is {sampleText}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithMissingSecret_ThrowsException()
    {
        
        // Arrange
        var secretKey = $"SECRET-{Guid.NewGuid():N}";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConnectorInputException>(() =>
            _sut.ReplaceSecretsAsync($"The value is {{{{secrets.{secretKey}}}}}"));
        Assert.Contains(secretKey, exception.Message);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_FallsBackToNextProvider_WhenFirstReturnsNull()
    {
        // Arrange
        var sharedKey = $"SECRET-{Guid.NewGuid():N}";
        var envValue = "environment-value";
        // AzureSecretProvider is registered first in fixture
        await _fixture.SeedEnvironmentVariableSecret(sharedKey, envValue);

        // Act
        var result = await _sut.ReplaceSecretsAsync($"Secret: {{{{ secrets.{sharedKey} }}}}");

        // Assert
        Assert.Equal($"Secret: {envValue}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_WithAzureKeyVaultProvider_ShouldResolveSecrets()
    {
        // Arrange
        var secretKey = $"SECRET-{Guid.NewGuid():N}";
        var testValue = $"test-value-{Guid.NewGuid():N}";
        await _fixture.SeedAzureSecret(secretKey, testValue);

        // Act
        var result = await _sut.ReplaceSecretsAsync($"The value is {{{{ secrets.{secretKey} }}}}");

        // Assert
        Assert.Equal($"The value is {testValue}", result);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_ShouldReplaceSecretsFromMultipleProviders()
    {
        // Arrange
        var envKey = $"ENV-SECRET-{Guid.NewGuid():N}";
        var envValue = $"env-value-{Guid.NewGuid():N}";
        var azureKey = "azure-key";
        var azureValue = "azure-value";
        await _fixture.SeedEnvironmentVariableSecret(envKey, envValue);
        await _fixture.SeedAzureSecret(azureKey, azureValue);
        
        // Act
        var azureResult = await _sut.ReplaceSecretsAsync($"Azure: {{{{ secrets.{azureKey} }}}}");
        var envResult = await _sut.ReplaceSecretsAsync($"Env: {{{{ secrets.{envKey} }}}}");
        var combinedResult = await _sut.ReplaceSecretsAsync(
            $"Combined: {{{{ secrets.{azureKey} }}}} and {{{{ secrets.{envKey} }}}}");

        // Assert
        Assert.Equal($"Azure: {azureValue}", azureResult);
        Assert.Equal($"Env: {envValue}", envResult);
        Assert.Equal($"Combined: {azureValue} and {envValue}", combinedResult);
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_ShouldRespectProviderOrder()
    {
        // Arrange
        var sharedKey = $"SECRET-{Guid.NewGuid():N}";
        var envValue = "environment-value";
        var azureValue = "azure-value";
        await _fixture.SeedEnvironmentVariableSecret(sharedKey, envValue);
        await _fixture.SeedAzureSecret(sharedKey, azureValue);
        _fixture.ConfigureServices(true,new Dictionary<string, string>
        {
            ["Zeebe:ConnectorSecrets:Providers:0"] = "EnvironmentVariablesSecretProvider",
            ["Zeebe:ConnectorSecrets:Providers:1"] = "AzureKeyVaultSecretProvider"
        });
        var envFirstHandler = _fixture.ServiceProvider.GetRequiredService<ISecretHandler>();

        
        // Act
        var resultAzureFirst = await _sut.ReplaceSecretsAsync($"Secret: {{{{ secrets.{sharedKey} }}}}");
        var resultEnvFirst = await envFirstHandler.ReplaceSecretsAsync($"Secret: {{{{ secrets.{sharedKey} }}}}");

        // Assert
        Assert.Equal($"Secret: {azureValue}", resultAzureFirst);
        Assert.Equal($"Secret: {envValue}", resultEnvFirst);
    }
    
    [Fact]
    public void DI_Registration_RegistersAllComponents()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Zeebe:ConnectorSecrets:Providers:0"] = "AzureKeyVaultSecretProvider",
                ["Zeebe:ConnectorSecrets:AzureKeyVault:VaultUri"] = "https://localhost",
                ["Zeebe:ConnectorSecrets:Providers:1"] = "EnvironmentVariablesSecretProvider",
                ["Zeebe:ConnectorSecrets:EnvironmentVariables:Prefix"] = "TEST_"
            })
            .Build();

        // Act
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddConnectorSecrets(configuration.GetSection("Zeebe"));
        services.AddEnvironmentSecretProvider(configuration.GetSection("Zeebe"));
        services.AddAzureKeyVaultSecretProvider(configuration.GetSection("Zeebe"));

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var secretHandler = serviceProvider.GetRequiredService<ISecretHandler>();
        Assert.NotNull(secretHandler);

        var aggregator = serviceProvider.GetRequiredService<SecretProviderAggregator>();
        Assert.NotNull(aggregator);

        var providers = serviceProvider.GetServices<ISecretProvider>().ToList();
        Assert.Contains(providers, p => p is EnvironmentVariablesSecretProvider);
        Assert.Contains(providers, p => p is AzureKeyVaultSecretProvider);
    }
}