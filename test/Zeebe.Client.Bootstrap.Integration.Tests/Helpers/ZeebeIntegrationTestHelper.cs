using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Bootstrap.Extensions;
using Zeebe.Client.Bootstrap.Integration.Tests.Stubs;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Helpers
{
    public class IntegrationTestHelper : IAsyncDisposable
    {
        public const string LatestZeebeVersion = "1.1.0";
        public const int ZeebePort = 26500;
        private readonly CancellationTokenSource cancellationTokenSource;
        private TestcontainersContainer zeebeContainer;
        private IHost host;
        private IZeebeClient zeebeClient;

        public IntegrationTestHelper(string zeebeVersion, HandleJobDelegate handleJobDelegate)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<IntegrationTestHelper>();

            cancellationTokenSource = new CancellationTokenSource();
            
            zeebeContainer = SetupZeebe(logger, cancellationTokenSource.Token, zeebeVersion);
            var zeebePort = zeebeContainer.GetMappedPublicPort(IntegrationTestHelper.ZeebePort);
            
            host = SetupHost(loggerFactory, cancellationTokenSource.Token, zeebePort, handleJobDelegate);

            zeebeClient = (IZeebeClient)host.Services.GetService(typeof(IZeebeClient));

            WaitUntilBrokerIsReady(zeebeClient, logger);
        }
        public IZeebeClient ZeebeClient { get { return zeebeClient; } }

        public async ValueTask DisposeAsync()
        {
            zeebeClient.Dispose();
            zeebeClient = null;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            await this.zeebeContainer.DisposeAsync();
            zeebeContainer = null;

            await host.StopAsync();
            host = null;
        }

        private static TestcontainersContainer SetupZeebe(ILogger logger, CancellationToken cancellationToken, string version)
        {
            TestcontainersSettings.Logger = logger;
            
            var container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage($"camunda/zeebe:{version}")
                .WithExposedPort(IntegrationTestHelper.ZeebePort)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(IntegrationTestHelper.ZeebePort))
                .Build();

            container.StartAsync(cancellationToken).Wait();

            return container;
        }

        private static IHost SetupHost(ILoggerFactory loggerFactory, CancellationToken cancellationToken, int zeebePort, HandleJobDelegate handleJobDelegate) {
            var host = Host
                .CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services            
                            .AddSingleton(loggerFactory)
                            .BootstrapZeebe(
                                options => { 
                                    options.Client = new ClientOptions() {
                                        GatewayAddress = $"0.0.0.0:{zeebePort}"
                                    };
                                    options.Worker = new WorkerOptions() 
                                    {
                                        MaxJobsActive = 1,
                                        TimeoutInMilliseconds = 10000,
                                        PollingTimeoutInMilliseconds = 100,
                                        PollIntervalInMilliseconds = 30000
                                    };
                                },
                                "Zeebe.Client.Bootstrap.Integration.Tests"
                            )
                            .Add(new ServiceDescriptor(typeof(HandleJobDelegate), handleJobDelegate));
                    })
                .Build();

            host.RunAsync(cancellationToken);

            return host;
        }

        private static void WaitUntilBrokerIsReady(IZeebeClient client, ILogger logger) => TryToConnectToBroker(client, logger).Wait();

        private static async Task TryToConnectToBroker(IZeebeClient client, ILogger logger)
        {
            var ready = false;
            do
            {
                try
                {
                    var topology = await client.TopologyRequest().Send();
                    ready = topology.Brokers[0].Partitions.Count == 1;
                }
                catch (Exception ex)
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