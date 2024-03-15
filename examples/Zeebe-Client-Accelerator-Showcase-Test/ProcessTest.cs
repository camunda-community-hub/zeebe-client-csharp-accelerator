using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;
using Zeebe.Client;
using Zeebe_Client_Accelerator_Showcase.Controllers;
using Zeebe_Client_Accelerator_Showcase_Test.testcontainers;
using static PleaseWait.Dsl;
using static PleaseWait.TimeUnit;

namespace Zeebe_Client_Accelerator_Showcase_Test
{
    public class ProcessTest : IClassFixture<IntegrationTestFactory<Program>>
    {

        private readonly IntegrationTestFactory<Program> _factory;
        private readonly BpmAssert _bpmAssert;
        private readonly IZeebeClient _zeebeClient;

        public ProcessTest(IntegrationTestFactory<Program> factory, ITestOutputHelper outputHelper)
        {
            factory.OutputHelper = outputHelper;
            _factory = factory;
            _bpmAssert = factory.Services.GetRequiredService<BpmAssert>();
            _zeebeClient = factory.Services.GetRequiredService<IZeebeClient>();
        }

        [Fact]
        public async Task TestHappyPathAsync()
        {
            // Given
            var client = _factory.CreateClient();
            var request = new ApplicationRequest()
            {
                ApplicantName = "John Doe"
            };

            // When
            var response = await client.PostAsJsonAsync("/application", request);

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var processInstanceKey = (await response.Content.ReadFromJsonAsync<ApplicationResponse>()).ProcessInstanceKey;
            _bpmAssert.WaitUntilProcessInstanceHasStarted(processInstanceKey);

            // wait for user task and complete
            _bpmAssert.WaitUntilProcessInstanceHasReachedElement(processInstanceKey, "Task_AppoveUser");

            var humanTask = await _zeebeClient.NewActivateJobsCommand().JobType("io.camunda.zeebe:userTask")
                .MaxJobsToActivate(1).WorkerName("Xunit").Timeout(TimeSpan.FromMinutes(5)).Send();
            var job = humanTask.Jobs.First();
            Assert.Equal(processInstanceKey, job.ProcessInstanceKey);
            Assert.Equal("Task_AppoveUser", job.ElementId);
            await _zeebeClient.NewCompleteJobCommand(job.Key).Variables("{\"approved\": true}").Send();

            // await user account creation and end of process
            _bpmAssert.WaitUntilProcessInstanceHasCompletedElement(processInstanceKey, "Activity_CreateUserAccount");
            _bpmAssert.WaitUntilProcessInstanceHasEnded(processInstanceKey);
            _bpmAssert.AssertThatProcessInstanceHasCompletedElement(processInstanceKey, "EndEvent_ApplicationApproved");
        }
    }
}