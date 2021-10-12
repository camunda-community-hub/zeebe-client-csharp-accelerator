using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Extensions;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Helpers
{
    public class IntegrationTestHelper : IAsyncDisposable
    {
        public const string LatestZeebeVersion = "1.1.0";
        public const int ZeebePort = 26500;
        private readonly ILogger<IntegrationTestHelper> logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly TestcontainersContainer zeebeContainer;
        private readonly IHost host;
        private readonly IZeebeClient zeebeClient;

        public IntegrationTestHelper(HandleJobDelegate handleJobDelegate)
            : this(LatestZeebeVersion, handleJobDelegate) { }

        public IntegrationTestHelper(string zeebeVersion, HandleJobDelegate handleJobDelegate)
        {
            var loggerFactory = LoggerFactory.Create(builder => 
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace));

            this.logger = loggerFactory.CreateLogger<IntegrationTestHelper>();

            cancellationTokenSource = new CancellationTokenSource();
            
            zeebeContainer = SetupZeebe(logger, zeebeVersion);
            
            host = SetupHost(loggerFactory, IntegrationTestHelper.ZeebePort, handleJobDelegate);

            zeebeClient = (IZeebeClient)host.Services.GetService(typeof(IZeebeClient));
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

            await this.zeebeContainer.StopAsync();
            await this.zeebeContainer.CleanUpAsync();
            await this.zeebeContainer.DisposeAsync();
        }

        private static TestcontainersContainer SetupZeebe(ILogger logger, string version)
        {
            TestcontainersSettings.Logger = logger;
            
            var container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage($"camunda/zeebe:{version}")
                .WithName("zeebe-testcontainer")
                .WithPortBinding(IntegrationTestHelper.ZeebePort)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(IntegrationTestHelper.ZeebePort))
                .Build();

            return container;
        }

        private static IHost SetupHost(ILoggerFactory loggerFactory, int zeebePort, HandleJobDelegate handleJobDelegate)
        {
            var host = Host
                .CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services            
                            .AddSingleton(loggerFactory)
                            .BootstrapZeebe(
                                options => { 
                                    options.Client = new ClientOptions() {
                                        GatewayAddress = $"127.0.0.1:{zeebePort}"
                                    };
                                    options.Worker = new WorkerOptions() 
                                    {
                                        MaxJobsActive = 1,
                                        TimeoutInMilliseconds = 10000,
                                        PollingTimeoutInMilliseconds = 10000,
                                        PollIntervalInMilliseconds = 30000
                                    };
                                },
                                typeof(IntegrationTestHelper).Assembly
                            )                            
                            .Add(new ServiceDescriptor(typeof(HandleJobDelegate), handleJobDelegate));
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