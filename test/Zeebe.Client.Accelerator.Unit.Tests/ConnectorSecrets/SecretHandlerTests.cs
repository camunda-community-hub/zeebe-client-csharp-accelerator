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
    public async Task ReplaceSecretsAsync_ShouldHandleExceptionInProvider()
    {
        // Arrange
        _fakeProvider.Setup(p => p.GetSecretAsync(_secretKey)).ThrowsAsync(new Exception());
        
        // Act
        var result = await _sut.ReplaceSecretsAsync($"Secret: {{{{ secrets.{_secretKey} }}}}");

        // Assert
        Assert.Equal($"Secret: {_secretValue}", result);
    }
    
    
    [Theory]
    [InlineData("Input: secrets.test", "test", "test","Input: test")]
    [InlineData("Input: secrets.TEST", "TEST", "TEST", "Input: TEST")]
    [InlineData("Input: secrets.A/B", "A/B", "A/B", "Input: A/B")]
    [InlineData("Input: secrets.A.B", "A.B", "A.B", "Input: A.B")]
    [InlineData("Input: {{secrets.TEST}}", "TEST", "valid", "Input: valid")]
    [InlineData("Input: {secrets.TEST}", "TEST", "valid", "Input: {valid}")]
    [InlineData("Input: secrets.TEST0", "TEST0", "valid", "Input: valid")]
    [InlineData("Input: secrets.TEST-0", "TEST-0", "valid", "Input: valid")]
    [InlineData("Input: secrets.TEST_0", "TEST_0", "valid", "Input: valid")]
    [InlineData("Input: secrets.TEST_TEST", "TEST_TEST", "valid", "Input: valid")]
    [InlineData("Input: secrets.a_b_c_d_e_f", "a_b_c_d_e_f", "a_b_c_d_e_f", "Input: a_b_c_d_e_f")]
    [InlineData("Input: secrets.a.b.c.d.e.f", "a.b.c.d.e.f", "a.b.c.d.e.f", "Input: a.b.c.d.e.f")]
    [InlineData("Input: secrets.TEST TEST", "TEST", "valid", "Input: valid TEST")]
    [InlineData("Input: secrets._TEST", "", "invalid", "Input: secrets._TEST")]
    [InlineData("Input: secrets./TEST", "", "invalid", "Input: secrets./TEST")]
    [InlineData("Input: secrets.-TEST", "", "invalid", "Input: secrets.-TEST")]
    [InlineData("Input: secrets..TEST", "", "invalid", "Input: secrets..TEST")]
    [InlineData("Input: secrets.", "", "invalid", "Input: secrets.")]
    [InlineData("Input: secrets..", "", "invalid", "Input: secrets..")]
    [InlineData("Input: secrets.?", "", "invalid", "Input: secrets.?")]
    [InlineData("Input: secret.test", "test", "invalid", "Input: secret.test")]
    [InlineData("Input: secrets secrets.key", "key", "value", "Input: secrets value")]
    public async Task ReplaceSecretsAsync_ShouldHandleDifferentInputPatterns(string input , string secretKey, string secretValue, string output)
    {
        // Arrange
        var sut = CreateSecretHandlerWithMocks((secretKey, secretValue));
      
        
        // Act
        var result = await _sut.ReplaceSecretsAsync(input);

        // Assert
        Assert.Equal(output, result);
    }
    
    [Theory]
    [InlineData("{\"field1\": \"secrets.KEY1\"}", "{\"field1\": \"VALUE1\"}")]
    [InlineData("{\"field1\": \"secrets.KEY1\", \"field2\": \"secrets.KEY2\"}", "{\"field1\": \"VALUE1\", \"field2\": \"VALUE2\"}")]
    [InlineData("{\"field1\": \"{{secrets.KEY1}}\"}", "{\"field1\": \"VALUE1\"}")]
    [InlineData("{\"field1\": \"{{secrets.KEY1}}\", \"field2\": \"{{secrets.KEY2}}\"}", "{\"field1\": \"VALUE1\", \"field2\": \"VALUE2\"}")]
    public async Task ReplaceSecretsAsync_WithJsonInput_ShouldReplaceSecrets(string input, string expected)
    {
        // Arrange
        var sut = CreateSecretHandlerWithMocks(
            ("KEY1", "VALUE1"), 
            ("KEY2", "VALUE2")
        );

        // Act
        var result = await sut.ReplaceSecretsAsync(input);

        // Assert
        Assert.Equal(expected, result);
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