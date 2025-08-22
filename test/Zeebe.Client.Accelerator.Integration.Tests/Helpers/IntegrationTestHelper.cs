using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Accelerator.ConnectorSecrets.Providers.EnvironmentVariables;
using Zeebe.Client.Accelerator.Extensions;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Helpers
{
    public class IntegrationTestHelper : IAsyncDisposable
    {
        public const string LatestZeebeVersion = "8.6.1";
        public const int ZeebePort = 26500;
        private readonly ILogger<IntegrationTestHelper> logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IContainer zeebeContainer;
        private readonly IHost host;
        private readonly IZeebeClient zeebeClient;

        public IntegrationTestHelper(HandleJobDelegate handleJobDelegate, bool includeSecretProvider = false)
            : this(LatestZeebeVersion, handleJobDelegate, includeSecretProvider) { }

        public IntegrationTestHelper(string zeebeVersion, HandleJobDelegate handleJobDelegate, bool includeSecretProvider)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace));

            this.logger = loggerFactory.CreateLogger<IntegrationTestHelper>();

            cancellationTokenSource = new CancellationTokenSource();

            zeebeContainer = SetupZeebe(logger, zeebeVersion);

            host = SetupHost(loggerFactory, IntegrationTestHelper.ZeebePort, handleJobDelegate, includeSecretProvider);

            var scope = host.Services.CreateScope();
            zeebeClient = (IZeebeClient)scope.ServiceProvider.GetService(typeof(IZeebeClient));
        }

        public IZeebeClient ZeebeClient { get { return zeebeClient; } }

        internal async Task InitializeAsync()
        {
            await this.zeebeContainer.StartAsync(this.cancellationTokenSource.Token);
            await host.StartAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            await WaitUntilBrokerIsReady(this.zeebeClient, this.logger);
        }

        public async ValueTask DisposeAsync()
        {
            await host.StopAsync();
            host.Dispose();

            zeebeClient.Dispose();

            await this.zeebeContainer.DisposeAsync();
        }

        private static IContainer SetupZeebe(ILogger logger, string version)
        {
            var container = new ContainerBuilder()
                .WithImage($"camunda/zeebe:{version}")
                .WithName("zeebe-testcontainer")
                .WithPortBinding(IntegrationTestHelper.ZeebePort)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(IntegrationTestHelper.ZeebePort))
                .WithCleanUp(true)
                .WithLogger(logger)
                .Build();

            return container;
        }

        private static IHost SetupHost(ILoggerFactory loggerFactory, int zeebePort, HandleJobDelegate handleJobDelegate,
            bool includeSecretProvider)
        {
            var host = Host
                .CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services
                            .AddSingleton(loggerFactory)
                            .BootstrapZeebe(
                                options =>
                                {
                                    options.Client = new ClientOptions()
                                    {
                                        GatewayAddress = $"127.0.0.1:{zeebePort}"
                                    };
                                    options.Worker = new WorkerOptions()
                                    {
                                        MaxJobsActive = 1,
                                        TimeoutInMilliseconds = 10000,
                                        PollingTimeoutInMilliseconds = 10000,
                                        PollIntervalInMilliseconds = 30000,
                                        RetryTimeoutInMilliseconds = 1000
                                    };
                                },
                                secretOptions =>
                                {
                                    if (includeSecretProvider)
                                    {
                                        secretOptions.Providers = new List<string>()
                                        {
                                            "EnvironmentVariablesSecretProvider"
                                        };  
                                    }
                                },
                                typeof(IntegrationTestHelper).Assembly
                            )
                            .Add(new ServiceDescriptor(typeof(HandleJobDelegate), handleJobDelegate));

                        if (includeSecretProvider)
                        {
                            var configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                    ["Zeebe:ConnectorSecrets:EnvironmentVariables:Prefix"] = "TEST_"
                                })
                                .Build();

                            services.AddEnvironmentSecretProvider(configuration.GetSection("Zeebe"));
                        }

                    })
                .Build();

            return host;
        }

        private static async Task WaitUntilBrokerIsReady(IZeebeClient client, ILogger logger)
        {
            var ready = false;
            do
            {
                try
                {
                    var topology = await client.TopologyRequest().Send();
                    ready = topology.Brokers[0].Partitions.Count == 1;
                }
                catch (System.Exception ex)
                {
                    logger.LogWarning(ex, "Error requesting topology.");
                    Thread.Sleep(1000);
                }

                logger.LogInformation("Zeebe not ready, retrying.");
            }
            while (!ready);
        }
    }
}