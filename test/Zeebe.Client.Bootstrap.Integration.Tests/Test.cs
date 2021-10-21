using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Extensions;
using Zeebe.Client.Bootstrap.Integration.Tests.Handlers;
using Zeebe.Client.Bootstrap.Integration.Tests.Helpers;

namespace Zeebe.Client.Bootstrap.Integration.Tests
{
    public class Test : IAsyncLifetime
    {
        private List<IJob> jobs;

        private readonly IntegrationTestHelper helper;

        public Test()
        {
            this.helper = new IntegrationTestHelper((job, cancellationToken) => this.jobs.Add(job));
        }

        [Fact]
        public async Task JobHandlerIsExecutedWhenProcesHasStarted()
        {   
            jobs = new List<IJob>();

            var zeebeClient = this.helper.ZeebeClient;            

            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(GetResourceFile("simple-test.bpmn"))
                .Send();

            Assert.True(deployResponse.Key > 0);

            var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("SimpleTest")
                .LatestVersion()
                .Send();

            Assert.NotNull(processInstance);
            
            WaitForHandlersToComplete(1, 1500);

            Assert.True(this.jobs.Count == 1);
        } 
        
        [Fact]
        public async Task BusinessExceptionIsCorrectlyPropegatedWhenProcesHasStarted()
        {            
            jobs = new List<IJob>();
            
            var zeebeClient = this.helper.ZeebeClient;            

            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(GetResourceFile("exception-test.bpmn"))
                .Send();

            Assert.True(deployResponse.Key > 0);

            var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("ExceptionTest")
                .LatestVersion()
                .Send();

            Assert.NotNull(processInstance);
            
            WaitForHandlersToComplete(2, 1500);

            Assert.True(this.jobs.Count == 2);
        }

        [Fact]
        public async Task InAndOutputVariablesAreCorrectlySerializedWhenProcesHasStarted()
        {
            var expectedGuid = Guid.NewGuid();

            jobs = new List<IJob>();

            var zeebeClient = this.helper.ZeebeClient;

            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(GetResourceFile("variables-test.bpmn"))
                .Send();

            Assert.True(deployResponse.Key > 0);

            var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("VariablesTest")
                .LatestVersion()
                .State(new {
                    Guid = expectedGuid
                })
                .Send();

            Assert.NotNull(processInstance);

            WaitForHandlersToComplete(2, 1500);

            Assert.True(this.jobs.Count == 2);

            var expected = OutputJobHandler.State;
            var actual = jobs[1] as InputJob;

            Assert.NotNull(actual);
            Assert.NotNull(actual.State);

            Assert.Equal(expected.Bool, actual.State.Bool);
            Assert.Equal(expected.Int, actual.State.Int);
            Assert.Equal(expected.Guid, expectedGuid);
            Assert.Equal(expected.DateTime, actual.State.DateTime);
            Assert.Equal(expected.Int, actual.State.Int);
            Assert.Equal(expected.String, actual.State.String);
            Assert.Equal(expected.Double, actual.State.Double);
        }

        public async Task InitializeAsync()
        {
            await this.helper.InitializeAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await this.helper.DisposeAsync();
        }

        private string GetResourceFile(string bpmn)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", bpmn);
        }

        private void WaitForHandlersToComplete(int jobCountWhenReady, int timeoutInMs)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);
            while(DateTime.Now < timeout && jobs.Count < jobCountWhenReady)
            {
                Thread.Sleep(100);
            }

            Thread.Sleep(500);
        }
    }
}