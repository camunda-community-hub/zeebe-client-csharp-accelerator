using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client;
using Io.Zeebe.Redis.Connect.Csharp.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MartinCostello.Logging.XUnit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace Zeebe_Client_Accelerator_Showcase_Test.testcontainers
{
    public sealed class IntegrationTestFactory<TProgram> : WebApplicationFactory<TProgram>, ITestOutputHelperAccessor where TProgram : class 
    {
        public ITestOutputHelper? OutputHelper { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configure =>
            {
                configure.Add(new ZeebeRedisConfigurationSource());
            })
            .ConfigureServices(services =>
            {
                services.AddZeebeRedis(options =>
                {
                    options.RedisConfigString = "127.0.0.1";
                    options.RedisPollIntervallMillis = 250;
                    options.RedisConsumerGroup = "Xunit";
                })
               .AddSingleton(ZeebeClient.Builder()
                .UseGatewayAddress("127.0.0.1:26500")
                .UsePlainText()
                .Build())
               .AddSingleton<BpmAssert>()
               .AddHostedService(p => p.GetRequiredService<BpmAssert>());
            })
            .ConfigureLogging(p => p.AddXUnit(this))
            .UseEnvironment("Development")
            ;
        }

        private sealed class ZeebeRedisConfigurationSource : IConfigurationSource
        {
            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return new ZeebeRedisConfigurationProvider();
            }
        }

        private sealed class ZeebeRedisConfigurationProvider : ConfigurationProvider
        {
            private static readonly TaskFactory TaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

            private ZeebeRedisContainer? _container;

            public override void Load()
            {
                // Until the asynchronous configuration provider is available,
                // we use the TaskFactory to spin up a new task that handles the work:
                // https://github.com/dotnet/runtime/issues/79193
                // https://github.com/dotnet/runtime/issues/36018
                TaskFactory.StartNew(LoadAsync)
                  .Unwrap()
                  .ConfigureAwait(false)
                  .GetAwaiter()
                  .GetResult();
            }

            public async Task LoadAsync()
            {
                // start test containers and wait until Zeebe is available
                _container = new ZeebeRedisContainer();
                await _container.StartAsync();
                await _container.WaitUntilBrokerIsReady();
            }
        }

    }
}
