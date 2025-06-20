using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets;

namespace Zeebe.Client.Accelerator.Integration.Tests.ConnectorSecrets;

public class SecretHandlerTests : IClassFixture<ConnectorSecretsFixture>
{
    
    private readonly ConnectorSecretsFixture _fixture;
    private readonly ISecretHandler _sut;
   
    public SecretHandlerTests(ConnectorSecretsFixture fixture)
    {
        _fixture = fixture;
        _sut = _fixture.ServiceProvider.GetRequiredService<ISecretHandler>();
    }

    [Fact]
    public async Task ReplaceSecretsAsync_WithEnvironmentVariablesProvider_ShouldResolveSecrets()
    {
        // Arrange
        var secretKey = $"SECRET_{Guid.NewGuid():N}";
        var testValue = $"test-value-{Guid.NewGuid():N}";
        await _fixture.SeedEnvironmentVariableSecret(secretKey, testValue);

        // Act
        var result = await _sut.ReplaceSecretsAsync($"The value is {{{{ secrets.{secretKey} }}}}");

        // Assert
        Assert.Equal($"The value is {testValue}", result);
    }

}