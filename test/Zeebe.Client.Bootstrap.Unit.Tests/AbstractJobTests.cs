using Xunit;
using System;
using Moq;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class AbstractJobTests
    {
        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobIsNull() 
        {
            Assert.Throws<ArgumentNullException>("job", () => new JobA(null));
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
            var variables = Guid.NewGuid().ToString();
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

            var job = new JobA(mock.Object);
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
        }

    }
}