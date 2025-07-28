using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.AzureKeyVault;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;

namespace Zeebe.Client.Accelerator.Integration.Tests.ConnectorSecrets;

public class ConnectorSecretsFixture : IAsyncLifetime
{
    public IContainer VaultContainer { get; private set; }
    public string VaultUri { get; private set; }
    public string IdentityUri { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }

    public async Task InitializeAsync()
    {
        VaultContainer = new ContainerBuilder()
            .WithImage($"nagyesta/lowkey-vault:3.2.0")
            .WithName("lowkey-vault-testcontainer")
            .WithPortBinding(8443, false)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8443))
            .WithCleanUp(true)
            .Build();

        await VaultContainer.StartAsync();

        var hostPort = VaultContainer.GetMappedPublicPort(8443);
        IdentityUri = $"http://localhost:{VaultContainer.GetMappedPublicPort(8080)}";
        VaultUri = $"https://localhost:{hostPort}";

        // Setup the environment for testing with Lowkey Vault
        Environment.SetEnvironmentVariable("IDENTITY_ENDPOINT", $"{IdentityUri}/metadata/identity/oauth2/token");
        Environment.SetEnvironmentVariable("IDENTITY_HEADER", "header");
    }

    public async Task DisposeAsync()
    {
        if (VaultContainer != null)
        {
            await VaultContainer.DisposeAsync();
        }
    }

    public async Task SeedAzureSecret(string key, string value)
    {
        var options = new SecretClientOptions();
        options.DisableChallengeResourceVerification = true;
        DisableSslValidation(options);

        var client = new SecretClient(
            new Uri(VaultUri),
            new DefaultAzureCredential(),
            options);

        await client.SetSecretAsync(key, value);
    }

    public async Task SeedEnvironmentVariableSecret(string key, string value)
    {
        var envKey = $"TEST_{key}";
        var testValue = value;
        Environment.SetEnvironmentVariable(envKey, testValue);
        await Task.CompletedTask;
    }

    public void ConfigureServices(Dictionary<string, string> configuration = null)
    {
        var providerConfig = new Dictionary<string, string>
        {
            ["Zeebe:ConnectorSecrets:AzureKeyVault:VaultUri"] = VaultUri,
            ["Zeebe:ConnectorSecrets:EnvironmentVariables:Prefix"] = "TEST_"
        };
        var mergedConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(providerConfig.Concat(configuration ?? new Dictionary<string, string>
            {
                ["Zeebe:ConnectorSecrets:Providers:0"] = "AzureKeyVaultSecretProvider",
                ["Zeebe:ConnectorSecrets:Providers:1"] = "EnvironmentVariablesSecretProvider",
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddConnectorSecrets(mergedConfiguration.GetSection("Zeebe"));
        services.AddEnvironmentSecretProvider(mergedConfiguration.GetSection("Zeebe"));
        services.AddSingleton<ISecretProvider>(sp =>
        {
            var options = Microsoft.Extensions.Options.Options.Create(
                new AzureKeyVaultSecretProviderOptions { VaultUri = VaultUri });
            var logger = sp.GetRequiredService<ILogger<AzureKeyVaultSecretProvider>>();

            var secretClientOptions = new SecretClientOptions();
            secretClientOptions.DisableChallengeResourceVerification = true;
            DisableSslValidation(secretClientOptions);
            return new AzureKeyVaultSecretProvider(options, logger, secretClientOptions);
        });

        ServiceProvider = services.BuildServiceProvider();
    }

    private void DisableSslValidation(ClientOptions options)
    {
        var clientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        options.Transport = new HttpClientTransport(clientHandler);
    }
}
