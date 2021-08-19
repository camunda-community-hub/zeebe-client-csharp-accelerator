using System;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            cancellationTokenSource = new CancellationTokenSource();
            
            zeebeContainer = SetupZeebe(cancellationTokenSource.Token, zeebeVersion);
            var zeebePort = zeebeContainer.GetMappedPublicPort(IntegrationTestHelper.ZeebePort);
            
            host = SetupHost(cancellationTokenSource.Token, zeebePort, handleJobDelegate);

            zeebeClient = (IZeebeClient)host.Services.GetService(typeof(IZeebeClient));

            //CHECK: No need for waiting because of the configured wait strategy?
            //WaitUntilBrokerIsReady(zeebeClient);
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

        private static TestcontainersContainer SetupZeebe(CancellationToken cancellationToken, string version)
        {
            var container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage($"camunda/zeebe:{version}")
                .WithName("camunda/zeebe")
                .WithExposedPort(IntegrationTestHelper.ZeebePort)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(IntegrationTestHelper.ZeebePort))
                .Build();

            container.StartAsync(cancellationToken).Wait();
            return container;
        }

        private static IHost SetupHost(CancellationToken cancellationToken, int zeebePort, HandleJobDelegate handleJobDelegate) {
            var host = Host
                .CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services                            
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

        private static void WaitUntilBrokerIsReady(IZeebeClient client) => TryToConnectToBroker(client).Wait();

        private static async Task TryToConnectToBroker(IZeebeClient client)
        {
            var ready = false;
            do
            {
                try
                {
                    var topology = await client.TopologyRequest().Send();
                    ready = topology.Brokers[0].Partitions.Count == 1;
                }
                catch (Exception)
                {
                    // retry
                    Thread.Sleep(500);
                }
            }
            while (!ready);
        }
    }
}