using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

namespace Zeebe.Client.Accelerator.Unit.Tests.ConnectorSecrets.Providers.EnvironmentVariables;

public class EnvironmentVariablesSecretProviderTests
{
    private readonly Mock<ILogger<EnvironmentVariablesSecretProvider>> _loggerMock = new();
    private readonly Mock<IOptions<EnvironmentVariablesSecretProviderOptions>> _optionsMock = new();

    [Fact]
    public async Task GetSecretAsync_EmptyKey_ShouldReturnNull()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns(new EnvironmentVariablesSecretProviderOptions());
        var provider = new EnvironmentVariablesSecretProvider(_loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await provider.GetSecretAsync(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecretAsync_WithPrefix_ShouldUseFullKey()
    {
        // Arrange
        const string prefix = "PREFIX_";
        const string key = "SECRET_KEY";
        const string expectedValue = "secret_value";
        Environment.SetEnvironmentVariable($"{prefix}{key}", expectedValue);
        
        _optionsMock.Setup(o => o.Value).Returns(new EnvironmentVariablesSecretProviderOptions { Prefix = prefix });
        var provider = new EnvironmentVariablesSecretProvider(_loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await provider.GetSecretAsync(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetSecretAsync_WithoutPrefix_ShouldUseKeyDirectly()
    {
        // Arrange
        const string key = "DIRECT_KEY";
        const string expectedValue = "direct_value";
        Environment.SetEnvironmentVariable(key, expectedValue);
        
        _optionsMock.Setup(o => o.Value)
            .Returns(new EnvironmentVariablesSecretProviderOptions { Prefix = string.Empty });
        var provider = new EnvironmentVariablesSecretProvider(_loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await provider.GetSecretAsync(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetSecretAsync_NonExistentKey_ShouldReturnNull()
    {
        // Arrange
        const string nonExistentKey = "NON_EXISTENT_KEY";

        _optionsMock.Setup(o => o.Value).Returns(new EnvironmentVariablesSecretProviderOptions());
        var provider = new EnvironmentVariablesSecretProvider(_loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await provider.GetSecretAsync(nonExistentKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns((EnvironmentVariablesSecretProviderOptions)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EnvironmentVariablesSecretProvider(_loggerMock.Object, _optionsMock.Object));
    }
}
