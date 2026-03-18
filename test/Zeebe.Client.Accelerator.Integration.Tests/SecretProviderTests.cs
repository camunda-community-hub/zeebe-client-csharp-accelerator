using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Extensions;
using Zeebe.Client.Accelerator.Integration.Tests.Handlers;
using Zeebe.Client.Accelerator.Integration.Tests.Helpers;
using Zeebe.Client.Accelerator.Abstractions;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Zeebe.Client.Accelerator.Integration.Tests
{
    [Collection("Sequential")]
    public class SecretProviderTests : IAsyncLifetime
    {
        private ITestOutputHelper _testOutputHelper;

        private List<IJob> jobs;

        private IntegrationTestHelper integrationTestHelper;

        private string jsonPropertyValue;

        public SecretProviderTests(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task InAndOutputVariablesAreCorrectlySerializedWithSecretsWhenProcesHasStarted()
        {
            var expectedGuid = Guid.NewGuid();
            var secretKey = $"SECRET-{Guid.NewGuid():N}";
            var testValue = $"test-value-{Guid.NewGuid():N}";
            var envKey = $"TEST_{secretKey}";
            Environment.SetEnvironmentVariable(envKey, testValue);
            var inputWithSecret = $"This is a secret : {{{{secrets.{secretKey}}}}}";
            var expectedValueWithReplacedSecret = $"This is a secret : {testValue}";
            OutputJobHandler.State.MyJsonPropertyNameWithSecret = inputWithSecret;

            jobs = new List<IJob>();
            integrationTestHelper =
                new IntegrationTestHelper((job, cancellationToken) => this.jobs.Add(job), includeSecretProvider: true);
            await integrationTestHelper.InitializeAsync();
            var zeebeClient = integrationTestHelper.ZeebeClient;

            var deployResponse = await zeebeClient.NewDeployCommand()
                .AddResourceFile(GetResourceFile("variables-test.bpmn"))
                .Send();



            var processInstance = await zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId("VariablesTest")
                .LatestVersion()
                .State(new
                {
                    Guid = expectedGuid
                })
                .Send();

            Assert.True(deployResponse.Key > 0);
            Assert.NotNull(processInstance);

            WaitForHandlersToComplete(2, 10000);

            Assert.True(this.jobs.Count == 2);

            var expected = OutputJobHandler.State;

            var actual = jobs[1] as ZeebeJob<InputState>;

            Assert.NotNull(actual);
            Assert.NotNull(actual.getVariables());
            var state = actual.getVariables();

            Assert.Equal(expected.Bool, state.Bool);
            Assert.Equal(expected.Int, state.Int);
            Assert.Equal(expected.Guid, expectedGuid);
            Assert.Equal(expected.DateTime, state.DateTime);
            Assert.Equal(expected.Int, state.Int);
            Assert.Equal(expected.String, state.String);
            Assert.Equal(expected.Double, state.Double);
            Assert.Null(state.ToBeIgnored);
            Assert.Equal(expected.MyJsonPropertyName, state.JsonPropertyNamedAttr);
            Assert.Equal(expectedValueWithReplacedSecret, state.JsonPropertyNamedAttrWithSecret);

            var doneMessage =
                zeebeClient.ReceiveMessage<DoneMessage>("responseFor_" + expectedGuid, TimeSpan.FromSeconds(5));
            Assert.Equal(expected.Guid, doneMessage.Guid);
            Assert.Equal(expected.DateTime, doneMessage.DateTime);

        }

        ValueTask IAsyncLifetime.InitializeAsync()
        {
            jsonPropertyValue = OutputJobHandler.State.MyJsonPropertyNameWithSecret;
            return ValueTask.CompletedTask;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (integrationTestHelper != null) await integrationTestHelper.DisposeAsync();
            foreach (var item in Environment.GetEnvironmentVariables().Keys)
            {
                Environment.SetEnvironmentVariable(item.ToString(), null);
            }
            OutputJobHandler.State.MyJsonPropertyNameWithSecret = jsonPropertyValue;
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

        private class DoneMessage
        {
            public Guid Guid { get; set; }
            public DateTime DateTime { get; set; }
        }

        private class MultiThreadVariables
        {
            public List<int> UsedThreads { get; set; }
        }

        private class UserTaskHeaders
        {
            [JsonPropertyName("io.camunda.zeebe:assignee")]
            public String Assignee { get; set; }
            [JsonPropertyName("io.camunda.zeebe:candidateGroups")]
            public String CandidateGroups { get; set; }
            [JsonPropertyName("io.camunda.zeebe:candidateUsers")]
            public String CandidateUsers { get; set; }

            public List<String> GetCandidateGroups()
            {
                if (CandidateGroups == null) { return new List<String>(); }
                return JsonSerializer.Deserialize<List<String>>(CandidateGroups);
            }
            public List<String> GetCandidateUsers()
            {
                if (CandidateUsers == null) { return new List<String>(); }
                return JsonSerializer.Deserialize<List<String>>(CandidateUsers);
            }

        }

        private class AcknowledgeUserTaskVariables
        {
            public Guid CorrelationId { get; set; }
        }
    }
}