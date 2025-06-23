using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets;

namespace Zeebe.Client.Accelerator.Unit.Tests.ConnectorSecrets;

public class SecretHandlerTests
{
    private readonly ISecretHandler _sut;
    private readonly Mock<FakeProvider> _fakeProvider = new ();
    private readonly Mock<FakeProvider> _fakeProvider2 = new ();
    private readonly string _secretKey = "SECRET-KEY";
    private readonly string _secretValue = "secret-value";

    public SecretHandlerTests()
    {
        _sut = CreateSecretHandlerWithMocks((_secretKey, _secretValue));
    }
    
    [Fact]
    public async Task ReplaceSecretsAsync_HandlesExceptionInProvider()
    {
        // Arrange
        _fakeProvider.Setup(p => p.GetSecretAsync(_secretKey)).ThrowsAsync(new Exception());
        
        // Act
        var result = await _sut.ReplaceSecretsAsync($"Secret: {{{{ secrets.{_secretKey} }}}}");

        // Assert
        Assert.Equal($"Secret: {_secretValue}", result);
    }
    
    
    private ISecretHandler CreateSecretHandlerWithMocks(params (string key, string value)[] secrets)
    {
        //
        foreach (var (key, value) in secrets)
        {
            _fakeProvider.Setup(p => p.GetSecretAsync(key)).ReturnsAsync(value);
        }
        foreach (var (key, value) in secrets)
        {
            _fakeProvider2.Setup(p => p.GetSecretAsync(key)).ReturnsAsync(value);
        }
        var options = Microsoft.Extensions.Options.Options.Create(new SecretOptions
        {
            Providers = new List<string> { "FakeProvider", "FakeProvider2" }
        });

        var aggregatorLogger = new Mock<ILogger<SecretProviderAggregator>>();
        var aggregator = new SecretProviderAggregator(
            aggregatorLogger.Object,
            options,
            new List<ISecretProvider>() {_fakeProvider.Object, _fakeProvider2.Object});

        var handlerLogger = new Mock<ILogger<SecretHandler>>();
        return new SecretHandler(aggregator, handlerLogger.Object);
    }
}

public abstract class FakeProvider : ISecretProvider
{
    public abstract Task<string> GetSecretAsync(string key);
        
    public string Name => GetType().Name;
}

public abstract class FakeProvider2 : ISecretProvider
{
    public abstract Task<string> GetSecretAsync(string key);
        
    public string Name => GetType().Name;
}