using System;
using System.Linq;
using Xunit;
using Zeebe.Client.Accelerator.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Accelerator.Unit.Tests.Stubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Commands;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;
using Zeebe.Client.Accelerator.Options;
using Microsoft.Extensions.Options;

namespace Zeebe.Client.Accelerator.Unit.Tests
{
    public class BootstrapJobHandlerTests 
    {
        private readonly CancellationToken cancellationToken;
        private readonly List<IJobHandlerInfo> jobHandlerInfoCollection;
        private readonly Mock<HandleJobDelegate> handleJobDelegateMock;
        private readonly Mock<IJobClient> jobClientMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IJobWorker> jobWorkerMock;
        private readonly Mock<IJobWorkerBuilderStep3> jobWorkerBuilderStep3Mock;
        private readonly Mock<IJobWorkerBuilderStep2> jobWorkerBuilderStep2Mock;
        private readonly Mock<IJobWorkerBuilderStep1> jobWorkerBuilderStep1Mock;
        private readonly Mock<ICompleteJobCommandStep1> completeJobCommandStep1Mock;
        private readonly Mock<IThrowErrorCommandStep2> throwErrorCommandStep2Mock;
        private readonly Mock<IThrowErrorCommandStep1> throwErrorCommandStep1Mock;
        private readonly Mock<IFailJobCommandStep2> failCommandStep2Mock;
        private readonly Mock<IFailJobCommandStep1> failCommandStep1Mock;
        private readonly Mock<IJobHandlerInfoProvider> jobHandlerInfoProviderMock;
        private readonly Mock<IZeebeVariablesSerializer> serializerMock;
        private readonly Mock<IZeebeVariablesDeserializer> deserializerMock;
        private readonly Mock<WorkerOptions> zeebeWorkerOptionsMock;
        private readonly Mock<ZeebeClientAcceleratorOptions> zeebeClientBootstrapOptionsMock;
        private readonly Mock<IOptions<ZeebeClientAcceleratorOptions>> optionsMock;
        private readonly Mock<ILogger<ZeebeJobHandler>> loggerMock;

        [Fact]
        public void ThrowsArgumentNullExceptionWhenServiceProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new ZeebeJobHandler(null, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobHandlerInfoProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("jobHandlerInfoProvider", () => new ZeebeJobHandler(this.serviceProviderMock.Object, null, this.serializerMock.Object, this.deserializerMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenSerializerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serializer", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, null, this.deserializerMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenDeserializerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("deserializer", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, null, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsIsNull() 
        {
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, null, this.loggerMock.Object));            
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueIsNull() 
        {
            this.optionsMock.SetupGet(m => m.Value).Returns((ZeebeClientAcceleratorOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, null, this.loggerMock.Object));            
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueWorkerIsNull() 
        {
            this.zeebeClientBootstrapOptionsMock.SetupGet(m => m.Worker).Returns((WorkerOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, null, this.loggerMock.Object));            
        }


        [Fact]
        public void ThrowsArgumentNullExceptionWhenLoggerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("logger", () => new ZeebeJobHandler(this.serviceProviderMock.Object, this.jobHandlerInfoProviderMock.Object, this.serializerMock.Object, this.deserializerMock.Object, this.optionsMock.Object, null));
        }

        [Fact]
        public async Task JobFailsWhenTheJobHandlerIsNotFound() 
        {
            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(Guid.NewGuid().ToString());
            jobMock.SetupGet(m => m.Key).Returns(new Random().Next());
            jobMock.SetupGet(m => m.Retries).Returns(3);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.failCommandStep1Mock.Verify(c => c.Retries(2), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.ErrorMessage("ArgumentNullException: Value cannot be null. (Parameter 'jobHandlerInfo')"), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.Send(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobFailsWhenTheJobHandlerServiceIsNotFound() 
        {
            var expected = jobHandlerInfoCollection.First();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expected.JobType);
            jobMock.SetupGet(m => m.Key).Returns(new Random().Next());
            jobMock.SetupGet(m => m.Retries).Returns(3);

            this.serviceProviderMock.Setup(m => m.GetService(It.IsAny<Type>())).Returns(null);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.failCommandStep1Mock.Verify(c => c.Retries(2), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.ErrorMessage("InvalidOperationException: There is no service of type Zeebe.Client.Accelerator.Unit.Tests.Stubs.JobHandlerA."), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.Send(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
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

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Once);
            this.completeJobCommandStep1Mock.Verify(c => c.Variables(It.IsAny<string>()), Times.Never);
            this.completeJobCommandStep1Mock.Verify(c => c.SendWithRetry(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobIsCompletedWithVariablesWhenJobIsHandledWithAResponse()
        {
            var random = new Random();
            PrepareAsyncJobHandlersWithResultFor<JobHandlerD, ResponseD>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            this.serializerMock.Setup(m => m.Serialize(It.IsAny<ResponseD>())).Returns(expectedSerializedResponse);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Once);
            this.jobClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Never);
            this.jobClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
            this.completeJobCommandStep1Mock.Verify(c => c.Variables(expectedSerializedResponse), Times.Once);
            this.completeJobCommandStep1Mock.Verify(c => c.SendWithRetry(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobThrowsErrorWhenJobIsHandledWithAsbtractJobException() 
        {
            var random = new Random();

            PrepareAsyncJobHandlersFor<JobHandlerE>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Never);
            this.jobClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Once);
            this.jobClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
            this.throwErrorCommandStep1Mock.Verify(c => c.ErrorCode("12345"), Times.Once);
            this.throwErrorCommandStep2Mock.Verify(c => c.ErrorMessage("54321"), Times.Once);
            this.throwErrorCommandStep2Mock.Verify(c => c.Send(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task JobFailsWhenJobIsHandledWithAnException() 
        {
            var random = new Random();

            PrepareJobHandlersFor<JobHandlerF>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);
            jobMock.SetupGet(m => m.Retries).Returns(3);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Once);
            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Never);
            this.jobClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Never);
            this.failCommandStep1Mock.Verify(c => c.Retries(2), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.ErrorMessage("Exception: 123456789109876543210"), Times.Once);
            this.failCommandStep2Mock.Verify(c => c.Send(It.IsAny<TimeSpan>(), cancellationToken), Times.Once);
        }

        [Fact]        
        public async Task VariablesAreDeserializedWhenJobImplementGenericAbstractJob() 
        {
            var random = new Random();
            var jobs = new List<IJob>();
            var expected = new JobGState() { 
                Guid = Guid.NewGuid(),
                DateTime = DateTime.Now
            };
            var expectedSerializedVariables = new ZeebeVariablesSerializer().Serialize(expected);

            handleJobDelegateMock.Setup(m => m.Invoke(It.IsAny<ZeebeJob<JobGState>>(), It.IsAny<CancellationToken>()))
                .Callback<IJob, CancellationToken>((j, c) => jobs.Add(j));

            deserializerMock.Setup(m => m.Deserialize<JobGState>(expectedSerializedVariables))
                .Returns(expected);

            PrepareAsyncJobHandlersFor<JobHandlerG, JobGState>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
 
            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);
            jobMock.SetupGet(m => m.Variables).Returns(expectedSerializedVariables);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            Assert.True(jobs.Count == 1);
            
            var job = jobs.Single() as ZeebeJob<JobGState>;            
            Assert.NotNull(job.getVariables());
            Assert.Equal(expected, job.getVariables());
        }


        [Fact]        
        public async Task RetryTimeOutIsUsedWhenJobIsCompleted() 
        {
            var random = new Random();
            PrepareAsyncJobHandlersFor<JobHandlerC>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedRetryTimeout = this.zeebeWorkerOptionsMock.Object.RetryTimeout;

            var jobMock = new Mock<IJob>();
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Once);
            this.jobClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Never);
            this.jobClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);
            this.completeJobCommandStep1Mock.Verify(c => c.SendWithRetry(expectedRetryTimeout, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]        
        public async Task RetryTimeOutIsUsedWhenJobErrorIsThrown() 
        {
            var random = new Random();

            PrepareAsyncJobHandlersFor<JobHandlerE>();
            var expectedHandler = jobHandlerInfoCollection.First();
            var expectedKey = random.Next();
            var expectedSerializedResponse = Guid.NewGuid().ToString();
            var expectedRetryTimeout = this.zeebeWorkerOptionsMock.Object.RetryTimeout;

            var jobMock = new Mock<IJob>();            
            jobMock.SetupGet(m => m.Type).Returns(expectedHandler.JobType);
            jobMock.SetupGet(m => m.Key).Returns(expectedKey);

            var handler = Create();

            await handler.HandleJob(jobClientMock.Object, jobMock.Object, cancellationToken);

            this.jobClientMock.Verify(c => c.NewCompleteJobCommand(expectedKey), Times.Never);
            this.jobClientMock.Verify(c => c.NewThrowErrorCommand(expectedKey), Times.Once);
            this.jobClientMock.Verify(c => c.NewFailCommand(expectedKey), Times.Never);            

            this.throwErrorCommandStep2Mock.Verify(c => c.Send(expectedRetryTimeout, It.IsAny<CancellationToken>()), Times.Once);
        }


        #region Prepare

        public BootstrapJobHandlerTests()
        {
            this.cancellationToken = new CancellationToken();

            this.jobHandlerInfoCollection = new List<IJobHandlerInfo>(
                new Type[] { typeof(JobHandlerA), typeof(JobHandlerC) }
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
            this.failCommandStep2Mock = CreateIFailJobCommandStep2Mock();
            this.failCommandStep1Mock = CreateIFailJobCommandStep1Mock(this.failCommandStep2Mock);
            this.jobClientMock = CreateIJobClientMock(this.jobWorkerBuilderStep1Mock, this.completeJobCommandStep1Mock, this.throwErrorCommandStep1Mock, this.failCommandStep1Mock);

            this.jobHandlerInfoProviderMock = CreateIJobHandlerProviderMock();
            
            this.serializerMock = CreateSerializerMock();
            this.deserializerMock = CreateDeserializerMock();

            this.zeebeWorkerOptionsMock = CreateZeebeWorkerOptionsMock();
            this.zeebeClientBootstrapOptionsMock = CreateZeebeClientBootstrapOptionsMock(this.zeebeWorkerOptionsMock);
            this.optionsMock = CreateOptionsMock(this.zeebeClientBootstrapOptionsMock);
            
            this.loggerMock = new Mock<ILogger<ZeebeJobHandler>>();
        }

        private ZeebeJobHandler Create() 
        {
            return new ZeebeJobHandler(
                this.serviceProviderMock.Object,
                this.jobHandlerInfoProviderMock.Object,
                this.serializerMock.Object,
                this.deserializerMock.Object,
                this.optionsMock.Object,
                this.loggerMock.Object
            );
        }

        private static Mock<IServiceProvider> CreateIServiceProviderMock(Mock<HandleJobDelegate> handleJobDelegateMock)
        {
            var mock =  new Mock<IServiceProvider>();

            mock.Setup(m => m.GetService(typeof(JobHandlerA))).Returns(new JobHandlerA(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerB))).Returns(new JobHandlerB(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerC))).Returns(new JobHandlerC(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerD))).Returns(new JobHandlerD(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerE))).Returns(new JobHandlerE(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerF))).Returns(new JobHandlerF(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerG))).Returns(new JobHandlerG(handleJobDelegateMock.Object));
            mock.Setup(m => m.GetService(typeof(JobHandlerH))).Returns(new JobHandlerH(handleJobDelegateMock.Object));

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

        private static Mock<IFailJobCommandStep1> CreateIFailJobCommandStep1Mock(Mock<IFailJobCommandStep2> failJobCommandStep2Mock)
        {
            var mock = new Mock<IFailJobCommandStep1>();

            mock.Setup(m => m.Retries(It.IsAny<int>())).Returns(failJobCommandStep2Mock.Object);

            return mock;
        }

        private static Mock<IFailJobCommandStep2> CreateIFailJobCommandStep2Mock()
        {
            var mock = new Mock<IFailJobCommandStep2>();

            mock.Setup(m => m.ErrorMessage(It.IsAny<string>())).Returns(mock.Object);

            return mock;
        }


        private static Mock<IJobClient> CreateIJobClientMock(Mock<IJobWorkerBuilderStep1> step1Mock, Mock<ICompleteJobCommandStep1> completeJobCommandStep1Mock, Mock<IThrowErrorCommandStep1> throwErrorCommandStep1Mock, Mock<IFailJobCommandStep1> failCommandStep1Mock)
        {
            var mock = new Mock<IJobClient>();

            mock.Setup(c => c.NewCompleteJobCommand(It.IsAny<long>())).Returns(completeJobCommandStep1Mock.Object);
            mock.Setup(c => c.NewThrowErrorCommand(It.IsAny<long>())).Returns(throwErrorCommandStep1Mock.Object);
            mock.Setup(c => c.NewFailCommand(It.IsAny<long>())).Returns(failCommandStep1Mock.Object);
         
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

        private static Mock<IOptions<ZeebeClientAcceleratorOptions>> CreateOptionsMock(Mock<ZeebeClientAcceleratorOptions> zeebeClientBootstrapOptionsMock)
        {
            var mock = new Mock<IOptions<ZeebeClientAcceleratorOptions>>();
            
            mock.SetupGet(m => m.Value).Returns(zeebeClientBootstrapOptionsMock.Object);

            return mock;
        }

        private static Mock<ZeebeClientAcceleratorOptions> CreateZeebeClientBootstrapOptionsMock(Mock<WorkerOptions> workerOptionsMock)
        {
            var mock = new Mock<ZeebeClientAcceleratorOptions>();

            mock.SetupGet(m => m.Worker).Returns(workerOptionsMock.Object);

            return mock;
        }

        private static Mock<WorkerOptions> CreateZeebeWorkerOptionsMock() 
        {            
            var mock = new Mock<WorkerOptions>();

            var random = new Random();

            mock.SetupGet(m => m.Name).Returns(Guid.NewGuid().ToString());
            mock.SetupGet(m => m.MaxJobsActive).Returns(random.Next(1, int.MaxValue));
            mock.SetupGet(m => m.PollingTimeout).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.PollInterval).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.Timeout).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.RetryTimeout).Returns(TimeSpan.FromMilliseconds(random.Next()));

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
            where T : IZeebeWorker
        {
            var jobHandlerType = typeof(T);
            this.jobHandlerInfoCollection.Clear();
            this.jobHandlerInfoCollection.Add(new JobHandlerInfo(
                    jobHandlerType
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(IZeebeWorker.HandleJob)))
                    .Single(),
                ServiceLifetime.Transient,
                typeof(T).GetTypeInfo().Name,
                "Test"
            ));
        }

        private void PrepareAsyncJobHandlersFor<T>()
            where T : IAsyncZeebeWorker
        {
            var jobHandlerType = typeof(T);
            this.jobHandlerInfoCollection.Clear();
            this.jobHandlerInfoCollection.Add(new JobHandlerInfo(
                    jobHandlerType
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(IAsyncZeebeWorker.HandleJob)))
                    .Single(),
                ServiceLifetime.Transient,
                typeof(T).GetTypeInfo().Name,
                "Test"
            ));
        }

        private void PrepareAsyncJobHandlersFor<T, TInput>()
            where T : IAsyncZeebeWorker<TInput> where TInput : class, new()
        {
            var jobHandlerType = typeof(T);
            this.jobHandlerInfoCollection.Clear();
            this.jobHandlerInfoCollection.Add(new JobHandlerInfo(
                    jobHandlerType
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(IAsyncZeebeWorker<TInput>.HandleJob)))
                    .Single(),
                ServiceLifetime.Transient,
                typeof(T).GetTypeInfo().Name,
                "Test"
            ));
        }

        #endregion
        private void PrepareAsyncJobHandlersWithResultFor<T, TResponse>()
            where T : IAsyncZeebeWorkerWithResult<TResponse> where TResponse: class
        {
            var jobHandlerType = typeof(T);
            this.jobHandlerInfoCollection.Clear();
            this.jobHandlerInfoCollection.Add(new JobHandlerInfo(
                    jobHandlerType
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(IAsyncZeebeWorkerWithResult<TResponse>.HandleJob)))
                    .Single(),
                ServiceLifetime.Transient,
                typeof(T).GetTypeInfo().Name,
                "Test"
            ));
        }

    }
}