using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zeebe.Client.Bootstrap.Integration.Tests.Helpers;

namespace Zeebe.Client.Bootstrap.Integration.Tests
{
    public class Test : IAsyncDisposable
    {
        private int jobHandledCounter = 0;
        private static readonly string DemoProcessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "simple-process.bpmn");

        private readonly IntegrationTestHelper helper;

        public Test()
        {
            this.helper = new IntegrationTestHelper(IntegrationTestHelper.LatestZeebeVersion, (client, job, cancellationToken) => jobHandledCounter++);            
        }

        public async ValueTask DisposeAsync()
        {
            await this.helper.DisposeAsync();
        }

        [Fact]
        public async Task JobHandlerIsExecutedWhenProcesHasStarted()
        {
            var zeebeClient = this.helper.ZeebeClient;

            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(DemoProcessPath)
                .Send();

            Assert.True(deployResponse.Key > 0);

            var startCounter = this.jobHandledCounter;

            var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("SimpleProcess")
                .LatestVersion()
                .Send();

            Assert.NotNull(processInstance);
            
            Thread.Sleep(500);
            
            Assert.True(this.jobHandledCounter > startCounter);        
        }
    }
}