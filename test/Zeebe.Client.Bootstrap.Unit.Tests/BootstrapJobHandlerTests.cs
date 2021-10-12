using System;
using System.Linq;
using Xunit;
using Zeebe.Client.Bootstrap.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Commands;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class BootstrapJobHandlerTests 
    {
        private readonly CancellationToken cancellationToken;
        private readonly List<IJobHandlerInfo> jobHandlerInfoCollection;
        private readonly Mock<HandleJobDelegate> handleJobDelegateMock;
        private readonly Mock<IZeebeClient> zeebeClientMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IJobWorker> jobWorkerMock;
        private readonly Mock<IJobWorkerBuilderStep3> jobWorkerBuilderStep3Mock;
        private readonly Mock<IJobWorkerBuilderStep2> jobWorkerBuilderStep2Mock;
        private readonly Mock<IJobWorkerBuilderStep1> jobWorkerBuilderStep1Mock;
        private readonly Mock<ICompleteJobCommandStep1> completeJobCommandStep1Mock;
        private readonly Mock<IThrowErrorCommandStep2> throwErrorCommandStep2Mock;
        private readonly Mock<IThrowErrorCommandStep1> throwErrorCommandStep1Mock;
        private readonly Mock<IJobHandlerInfoProvider> jobHandlerInfoProviderMock;
        private readonly Mock<IZeebeVariablesSerializer> serializerMock;
        private readonly Mock<IZeebeVariablesDeserializer> deserializerMock;
        private readonly Mock<ILogger<BootstrapJobHandler>> loggerMock;

        [Fact]
        public void ThrowsArgumentNullExceptionWhenServiceProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new BootstrapJobHandler(null, this.zeebeClientMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenClientIsNull() 
        {
            Assert.Throws<ArgumentNullException>("client", () => new BootstrapJobHandler(this.serviceProviderMock.Object, null, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobHandlerInfoProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("jobHandlerInfoProvider", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, null, this.serializerMock.Object, this.deserializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenSerializerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serializer", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerInfoProviderMock.Object, null, this.deserializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenDeserializerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("deserializer", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, null, this.loggerMock.Object));
        }


        [Fact]
        public void ThrowsArgumentNullExceptionWhenLoggerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("logger", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, null));
        }

        [Fact]
        public async Task ThrowsArgumentNullExceptionWhenTheJobHandlerIsNotFound() 
        {
            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(Guid.NewGuid().ToString());

            var handler = Create();

            await Assert.ThrowsAsync<ArgumentNullException>("jobHandlerInfo",() => handler.HandleJob(jobMock.Object, cancellationToken));
        }

        [Fact]
        public async Task ThrowsInvalidOperationExceptionWhenTheJobHandlerServiceIsNotFound() 
        {
            var expected = jobHandlerInfoCollection.First();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expected.JobType);

            this.serviceProviderMock.Setup(m => m.GetService(It.IsAny<Type>())).Returns(null);

            var handler = Create();

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleJob(jobMock.Object, cancellationToken));
        }

        [Fact]
        public async Task JobIsCompletedWhenJobIsHandled() 
        {
            var random = new Random();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            await handler.HandleJob(jobMock.Object, cancellationToken);

            this.zeebeClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Once);
            this.completeJobCommandStep1Mock.Verify(c => c.Variables(It.IsAny<string>()), Times.Never);
            this.completeJobCommandStep1Mock.Verify(c => c.Send(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobIsCompletedWithVariablesWhenJobIsHandledWithAResponse()
        {
            var random = new Random();
            PrepareJobHandlersFor<JobD>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            this.serializerMock.Setup(m => m.Serialize(It.IsAny<ResponseD>())).Returns(expectedSerializedResponse);

            var handler = Create();

            await handler.HandleJob(jobMock.Object, cancellationToken);

            this.zeebeClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Once);
            this.zeebeClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Never);
            this.zeebeClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
            this.completeJobCommandStep1Mock.Verify(c => c.Variables(expectedSerializedResponse), Times.Once);
            this.completeJobCommandStep1Mock.Verify(c => c.Send(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobThrowsErrorWhenJobIsHandledWithAsbtractJobException() 
        {
            var random = new Random();

            PrepareJobHandlersFor<JobE>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            await handler.HandleJob(jobMock.Object, cancellationToken);

            this.zeebeClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Never);
            this.zeebeClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Once);
            this.zeebeClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
            this.throwErrorCommandStep1Mock.Verify(c => c.ErrorCode("12345"), Times.Once);
            this.throwErrorCommandStep2Mock.Verify(c => c.ErrorMessage("54321"), Times.Once);
        }

        [Fact]
        public async Task ThrowsExceptionWhenJobIsHandledWithAnException() 
        {
            var random = new Random();

            PrepareJobHandlersFor<JobF>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            try
            {
                await handler.HandleJob(jobMock.Object, cancellationToken);
            }
            catch(Exception ex) 
            {
                Assert.Equal("123456789109876543210", ex.InnerException.Message);
            }

            this.zeebeClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Never);
            this.zeebeClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Never);
            this.zeebeClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
        }

        [Fact]        
        public async Task VariablesAreDeserializedWhenJobImplementGenericAbstractJob() 
        {
            var random = new Random();
            var jobs = new List<IJob>();
            var expected = new JobGState();
            var expectedVariables = Guid.NewGuid().ToString();

            handleJobDelegateMock.Setup(m => m.Invoke(It.IsAny<JobG>(), It.IsAny<CancellationToken>()))
                .Callback<IJob, CancellationToken>((j, c) => jobs.Add(j));

            deserializerMock.Setup(m => m.Deserialize(expectedVariables, typeof(JobGState)))
                .Returns(expected);

            PrepareJobHandlersFor<JobG>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);
            jobMock.SetupGet(m => m.Variables).Returns(expectedVariables);

            var handler = Create();

            await handler.HandleJob(jobMock.Object, cancellationToken);

            Assert.True(jobs.Count == 1);
            
            var job = jobs.Single() as JobG;            
            Assert.NotNull(job.State);
            Assert.Equal(expected, job.State);
        }


        #region Prepare

        public BootstrapJobHandlerTests()
        {            
            this.cancellationToken = new CancellationToken();

            this.jobHandlerInfoCollection = new List<IJobHandlerInfo>(
                new Type[] { typeof(JobHandlerA), typeof(JobHandlerB) }
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                    .Select(m => CreateJobHandlerReference(m))
            );

            this.handleJobDelegateMock = new Mock<HandleJobDelegate>();
            this.serviceProviderMock = CreateIServiceProviderMock(this.handleJobDelegateMock);

            this.jobWorkerMock = CreateIJobWorkerMock();
            this.jobWorkerBuilderStep3Mock = CreateIJobWorkerBuilderStep3Mock(this.jobWorkerMock);
            this.jobWorkerBuilderStep2Mock = CreateIJobWorkerBuilderStep2Mock(this.jobWorkerBuilderStep3Mock);
            this.jobWorkerBuilderStep1Mock = CreateIJobWorkerBuilderStep1Mock(this.jobWorkerBuilderStep2Mock);            
            this.completeJobCommandStep1Mock = CreateICompleteJobCommandStep1Mock();
            this.throwErrorCommandStep2Mock = CreateIThrowErrorCommandStep2Mock();
            this.throwErrorCommandStep1Mock = CreateIThrowErrorCommandStep1Mock(this.throwErrorCommandStep2Mock);
            this.zeebeClientMock = CreateIZeebeClientMock(this.jobWorkerBuilderStep1Mock, this.completeJobCommandStep1Mock, this.throwErrorCommandStep1Mock);

            this.jobHandlerInfoProviderMock = CreateIJobHandlerProviderMock();
            
            this.serializerMock = CreateSerializerMock();
            this.deserializerMock = CreateDeserializerMock();
            
            this.loggerMock = new Mock<ILogger<BootstrapJobHandler>>();
        }

        private BootstrapJobHandler Create() 
        {
             return new BootstrapJobHandler(
                this.serviceProviderMock.Object, 
                this.zeebeClientMock.Object, 
                this.jobHandlerInfoProviderMock.Object,
                this.serializerMock.Object,
                this.deserializerMock.Object,
                this.loggerMock.Object
            );
        }

        private static Mock<IServiceProvider> CreateIServiceProviderMock(Mock<HandleJobDelegate> handleJobDelegateMock)
        {
            var mock =  new Mock<IServiceProvider>();

            mock.Setup(m => m.GetService(typeof(JobHandlerA))).Returns(new JobHandlerA(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerB))).Returns(new JobHandlerB(handleJobDelegateMock.Object));
            
            return mock;
        }

        private static Mock<ICompleteJobCommandStep1> CreateICompleteJobCommandStep1Mock() 
        {
            var mock = new Mock<ICompleteJobCommandStep1>();

            mock.Setup(m => m.Variables(It.IsAny<string>())).Returns(mock.Object);

            return mock;
        }

        private static Mock<IThrowErrorCommandStep1> CreateIThrowErrorCommandStep1Mock(Mock<IThrowErrorCommandStep2> throwErrorCommandStep2Mock) 
        {
            var mock = new Mock<IThrowErrorCommandStep1>();

            mock.Setup(m => m.ErrorCode(It.IsAny<string>())).Returns(throwErrorCommandStep2Mock.Object);

            return mock;
        }

        private static Mock<IThrowErrorCommandStep2> CreateIThrowErrorCommandStep2Mock() 
        {
            var mock = new Mock<IThrowErrorCommandStep2>();

            mock.Setup(m => m.ErrorMessage(It.IsAny<string>())).Returns(mock.Object);

            return mock;
        }

        private static Mock<IZeebeClient> CreateIZeebeClientMock(Mock<IJobWorkerBuilderStep1> step1Mock, Mock<ICompleteJobCommandStep1> completeJobCommandStep1Mock, Mock<IThrowErrorCommandStep1> throwErrorCommandStep1Mock)
        {
            var mock = new Mock<IZeebeClient>();

            mock.Setup(c => c.NewWorker()).Returns(step1Mock.Object);
            mock.Setup(c => c.NewCompleteJobCommand(It.IsAny<long>())).Returns(completeJobCommandStep1Mock.Object);
            mock.Setup(c => c.NewThrowErrorCommand(It.IsAny<long>())).Returns(throwErrorCommandStep1Mock.Object);
         
            return mock;
        }

        private static Mock<IJobWorkerBuilderStep1> CreateIJobWorkerBuilderStep1Mock(Mock<IJobWorkerBuilderStep2> step2Mock) {
            var mock = new Mock<IJobWorkerBuilderStep1>();

            mock.Setup(b => b.JobType(It.IsAny<string>())).Returns(step2Mock.Object);

            return mock;
        }

         private static Mock<IJobWorkerBuilderStep2> CreateIJobWorkerBuilderStep2Mock(Mock<IJobWorkerBuilderStep3> step3Mock) {
            var mock = new Mock<IJobWorkerBuilderStep2>();

            mock.Setup(b => b.Handler(It.IsAny<AsyncJobHandler>())).Returns(step3Mock.Object);

            return mock;
        }

        private static Mock<IJobWorkerBuilderStep3> CreateIJobWorkerBuilderStep3Mock(Mock<IJobWorker> jobWorkerMock) {
            var mock = new Mock<IJobWorkerBuilderStep3>();
            var builder = mock.Object;

            mock.Setup(b => b.FetchVariables(It.IsAny<IList<string>>())).Returns(builder);
            mock.Setup(b => b.FetchVariables(It.IsAny<string[]>())).Returns(builder);
            mock.Setup(b => b.MaxJobsActive(It.IsAny<int>())).Returns(builder);
            mock.Setup(b => b.Name(It.IsAny<string>())).Returns(builder);
            mock.Setup(b => b.PollingTimeout(It.IsAny<TimeSpan>())).Returns(builder);
            mock.Setup(b => b.PollInterval(It.IsAny<TimeSpan>())).Returns(builder);
            mock.Setup(b => b.Timeout(It.IsAny<TimeSpan>())).Returns(builder);
            mock.Setup(b => b.Open()).Returns(jobWorkerMock.Object);

            return mock;
        }

        private static Mock<IJobWorker> CreateIJobWorkerMock() {
            var mock = new Mock<IJobWorker>();

            return mock;
        }

        private Mock<IJobHandlerInfoProvider> CreateIJobHandlerProviderMock()
        {
            var mock = new Mock<IJobHandlerInfoProvider>();            

            mock.SetupGet(p => p.JobHandlerInfoCollection).Returns(this.jobHandlerInfoCollection);

            return mock;
        }

        private static Mock<IZeebeVariablesSerializer> CreateSerializerMock()
        {
            var mock = new Mock<IZeebeVariablesSerializer>();

            return mock;
        }

        private Mock<IZeebeVariablesDeserializer> CreateDeserializerMock()
        {
            var mock = new Mock<IZeebeVariablesDeserializer>();

            return mock;
        }

        private static IJobHandlerInfo CreateJobHandlerReference(MethodInfo handler) 
        {
            var random = new Random();
            
            return  new JobHandlerInfo(
                handler,
                ServiceLifetime.Scoped,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                random.Next(1, int.MaxValue),
                TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue)),
                TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue)),
                TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue)),
                new string[] {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString()
                }
            );
        }

        private void PrepareJobHandlersFor<T>() 
            where T : AbstractJob
        {
            this.jobHandlerInfoCollection.Clear();
            this.jobHandlerInfoCollection.Add(new JobHandlerInfo(
                typeof(JobHandlerB)
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(JobHandlerB.HandleJob)) && m.GetParameters()[0].ParameterType == typeof(T))
                    .Single(),
                ServiceLifetime.Transient,
                typeof(T).GetTypeInfo().Name,
                "Test"
            ));
        }

        #endregion
    }
}