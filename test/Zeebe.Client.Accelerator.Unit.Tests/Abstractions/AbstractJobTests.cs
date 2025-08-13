using Xunit;
using System;
using Moq;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Unit.Tests.Stubs;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Unit.Tests.Abstractions
{
    public class AbstractJobTests
    {
        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobIsNull() 
        {
            Assert.Throws<ArgumentNullException>("job", () => new ZeebeJob(null, null, new ZeebeVariablesDeserializer(), null));
        }

        [Fact]
        public void ProvidesAccessToJobClient()
        {
            var jobClientMock = new Mock<IJobClient>();
            var job = new ZeebeJob(jobClientMock.Object, new Mock<IJob>().Object, new ZeebeVariablesDeserializer(), new Mock<ISecretHandler>().Object);
            Assert.Equal(jobClientMock.Object, job.GetClient());
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var key = (long)random.Next();
            var type = Guid.NewGuid().ToString();
            var processInstanceKey = (long)random.Next();
            var bpmnProcessId = Guid.NewGuid().ToString();
            var processDefinitionVersion = random.Next();
            var processDefinitionKey = (long)random.Next();
            var elementId = Guid.NewGuid().ToString();
            var elementInstanceKey = (long)random.Next();
            var worker = Guid.NewGuid().ToString();
            var retries = random.Next();
            var deadline = new DateTime((long)random.Next());
            var state = new JobGState() { Guid = Guid.NewGuid() };
            var variables = new ZeebeVariablesSerializer().Serialize(state);
            var customHeaders = Guid.NewGuid().ToString();

            var mock = new Mock<IJob>();
            mock.SetupGet(j => j.Key).Returns(key);
            mock.SetupGet(j => j.Type).Returns(type);
            mock.SetupGet(j => j.ProcessInstanceKey).Returns(processInstanceKey);
            mock.SetupGet(j => j.BpmnProcessId).Returns(bpmnProcessId);
            mock.SetupGet(j => j.ProcessDefinitionVersion).Returns(processDefinitionVersion);
            mock.SetupGet(j => j.ProcessDefinitionKey).Returns(processDefinitionKey);
            mock.SetupGet(j => j.ElementId).Returns(elementId);
            mock.SetupGet(j => j.ElementInstanceKey).Returns(elementInstanceKey);
            mock.SetupGet(j => j.Worker).Returns(worker);
            mock.SetupGet(j => j.Retries).Returns(retries);
            mock.SetupGet(j => j.Deadline).Returns(deadline);
            mock.SetupGet(j => j.Variables).Returns(variables);
            mock.SetupGet(j => j.CustomHeaders).Returns(customHeaders);

            var secretHandlerMock = new Mock<ISecretHandler>();
            secretHandlerMock.Setup(s=>s.ReplaceSecretsAsync(It.IsAny<string>()))
                .ReturnsAsync(variables);
            var job = new ZeebeJob(null, mock.Object, new ZeebeVariablesDeserializer(), secretHandlerMock.Object);
            Assert.Equal(key, job.Key);
            Assert.Equal(type, job.Type);
            Assert.Equal(processInstanceKey, job.ProcessInstanceKey);
            Assert.Equal(bpmnProcessId, job.BpmnProcessId);
            Assert.Equal(processDefinitionVersion, job.ProcessDefinitionVersion);
            Assert.Equal(processDefinitionKey, job.ProcessDefinitionKey);
            Assert.Equal(elementId, job.ElementId);
            Assert.Equal(elementInstanceKey, job.ElementInstanceKey);
            Assert.Equal(worker, job.Worker);
            Assert.Equal(retries, job.Retries);
            Assert.Equal(deadline, job.Deadline);
            Assert.Equal(variables, job.Variables);
            Assert.Equal(customHeaders, job.CustomHeaders);
            
            var genericJob = new ZeebeJob<JobGState>(null, mock.Object, new ZeebeVariablesDeserializer(), secretHandlerMock.Object);
            Assert.Equal(key, genericJob.Key);
            Assert.Equal(type, genericJob.Type);
            Assert.Equal(processInstanceKey, genericJob.ProcessInstanceKey);
            Assert.Equal(bpmnProcessId, genericJob.BpmnProcessId);
            Assert.Equal(processDefinitionVersion, genericJob.ProcessDefinitionVersion);
            Assert.Equal(processDefinitionKey, genericJob.ProcessDefinitionKey);
            Assert.Equal(elementId, genericJob.ElementId);
            Assert.Equal(elementInstanceKey, genericJob.ElementInstanceKey);
            Assert.Equal(worker, genericJob.Worker);
            Assert.Equal(retries, genericJob.Retries);
            Assert.Equal(deadline, genericJob.Deadline);
            Assert.Equal(variables, genericJob.Variables);
            Assert.Equal(customHeaders, genericJob.CustomHeaders);
            Assert.Equal(state, genericJob.getVariables());
        }

    }
}