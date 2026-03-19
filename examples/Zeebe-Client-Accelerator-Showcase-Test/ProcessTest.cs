using Azure;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Zeebe.Client;
using Zeebe_Client_Accelerator_Showcase.Controllers;
using Zeebe_Client_Accelerator_Showcase_Test.testcontainers;

//[assembly: CaptureConsole]
namespace Zeebe_Client_Accelerator_Showcase_Test
{
    public class ProcessTest : IClassFixture<IntegrationTestFactory<Program>>
    {

        private readonly IntegrationTestFactory<Program> _factory;
        private readonly BpmAssert _bpmAssert;
        private readonly IZeebeClient _zeebeClient;
        private readonly HttpClient _zeebeHttpClient;

        public ProcessTest(IntegrationTestFactory<Program> factory, ITestOutputHelper outputHelper)
        {
            factory.OutputHelper = outputHelper;
            _factory = factory;
            _bpmAssert = factory.Services.GetRequiredService<BpmAssert>();
            _zeebeClient = factory.Services.GetRequiredService<IZeebeClient>();
            _zeebeHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:8080"),
            };
            _zeebeHttpClient.DefaultRequestHeaders.Accept.Clear();
            _zeebeHttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task TestHappyPathAsync()
        {
            // Given
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var request = new ApplicationRequest()
            {
                ApplicantName = "John Doe"
            };

            // When
            var response = await client.PostAsync("/application", ToJsonContent(request));

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var processInstanceKey = (await response.Content.ReadFromJsonAsync<ApplicationResponse>()).ProcessInstanceKey;
            _bpmAssert.WaitUntilProcessInstanceHasStarted(processInstanceKey);

            // wait for user task
            _bpmAssert.WaitUntilProcessInstanceHasReachedElement(processInstanceKey, "Task_AppoveUser");

            // complete the user task
            FindAndCompleteUserTask(processInstanceKey, "Task_AppoveUser", new
            {
                approved = true,
            });

            // await user account creation and end of process
            _bpmAssert.WaitUntilProcessInstanceHasCompletedElement(processInstanceKey, "Activity_CreateUserAccount");
            _bpmAssert.WaitUntilProcessInstanceHasEnded(processInstanceKey);
            _bpmAssert.AssertThatProcessInstanceHasCompletedElement(processInstanceKey, "EndEvent_ApplicationApproved");
        }

        private async void FindAndCompleteUserTask(long processInstanceKey, string taskName, object payload)
        {
            var userTask = _bpmAssert.AssertThatUserTaskExistsAndReturnValue(processInstanceKey, taskName);
            var completePayload = new
            {
                variables = payload
            };
            var userTasksResponse = await _zeebeHttpClient.PostAsync($"/v2/user-tasks/{userTask.UserTaskKey}/completion", ToJsonContent(completePayload));
            Assert.Equal(HttpStatusCode.NoContent, userTasksResponse.StatusCode);

        }

        private StringContent ToJsonContent(object? request)
        {
            var json = JsonConvert.SerializeObject(request);
            var completeContent = new StringContent(json, Encoding.UTF8, "application/json");
            return completeContent;
        }
    }
}