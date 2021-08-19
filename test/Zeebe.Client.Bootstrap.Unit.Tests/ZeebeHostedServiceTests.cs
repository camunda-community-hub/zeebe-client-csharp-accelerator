using System;
using System.Linq;
using Xunit;
using Zeebe.Client.Bootstrap.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Bootstrap.Options;
using Microsoft.Extensions.Options;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class ZeebeHostedServiceTests 
    {
        private readonly CancellationToken cancellationToken;
        private readonly List<IJobHandlerReference> jobHandlers;
        private readonly Mock<HandleJobDelegate> handleJobDelegateMock;
        private readonly Mock<IZeebeClient> zeebeClientMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IJobWorker> jobWorkerMock;
        private readonly Mock<IJobWorkerBuilderStep3> jobWorkerBuilderStep3Mock;
        private readonly Mock<IJobWorkerBuilderStep2> jobWorkerBuilderStep2Mock;
        private readonly Mock<IJobWorkerBuilderStep1> jobWorkerBuilderStep1Mock;
        private readonly Mock<IJobHandlerProvider> jobHandlerProviderMock;
        private readonly Mock<IOptions<ZeebeClientBootstrapOptions>> optionsMock;
        private readonly Mock<ZeebeClientBootstrapOptions> zeebeClientBootstrapOptionsMock;
        private readonly Mock<WorkerOptions> zeebeWorkerOptionsMock;
        private readonly Mock<ILogger<ZeebeHostedService>> loggerMock;

        [Fact]
        public void ThrowsArgumentNullExceptionWhenServiceProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new ZeebeHostedService(null, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenClientIsNull() 
        {
            Assert.Throws<ArgumentNullException>("client", () => new ZeebeHostedService(this.serviceProviderMock.Object, null, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobHandlerProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("jobHandlerProvider", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, null, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsIsNull() 
        {
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, null, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueIsNull() 
        {
            this.optionsMock.SetupGet(m => m.Value).Returns((ZeebeClientBootstrapOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueWorkerIsNull() 
        {
            this.zeebeClientBootstrapOptionsMock.SetupGet(m => m.Worker).Returns((WorkerOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenMaxJobsActiveIsSmallerOrEqualThen0(int maxJobsActive) 
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.MaxJobsActive).Returns(maxJobsActive);
            Assert.Throws<ArgumentOutOfRangeException>("WorkerOptions.MaxJobsActive", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenTimeoutIsSmallerOrEqualThen0(int timeout) 
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.Timeout).Returns(TimeSpan.FromMilliseconds(timeout));
            Assert.Throws<ArgumentOutOfRangeException>("WorkerOptions.Timeout", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollIntervalIsSmallerOrEqualThen0(int pollInterval) 
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.PollInterval).Returns(TimeSpan.FromMilliseconds(pollInterval));
            Assert.Throws<ArgumentOutOfRangeException>("WorkerOptions.PollInterval", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollingTimeoutIsSmallerOrEqualThen0(int pollingTimeout) 
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.PollingTimeout).Returns(TimeSpan.FromMilliseconds(pollingTimeout));
            Assert.Throws<ArgumentOutOfRangeException>("WorkerOptions.PollingTimeout", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenNameIsEmpty(string name) 
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.Name).Returns(name);
            Assert.Throws<ArgumentException>("WorkerOptions.Name", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenLoggerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("logger", () => new ZeebeHostedService(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.optionsMock.Object, null));
        }

        [Fact]
        public async Task WorkersAreCreatedWhenStartIsExecuted()
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            
            this.zeebeClientMock.Verify(c => c.NewWorker(), Times.Exactly(this.jobHandlers.Count));
        }

        [Fact]
        public async Task WorkerPropertiesAreSetWhenCreated()
        {
            zeebeWorkerOptionsMock.Setup(m => m.Name).Returns((string)null);

            var service = Create();
            await service.StartAsync(cancellationToken);
            
            this.jobHandlers.ForEach(handler => {
                this.jobWorkerBuilderStep1Mock.Verify(c => c.JobType(handler.JobType), Times.Once);
                this.jobWorkerBuilderStep2Mock.Verify(c => c.Handler(It.IsAny<AsyncJobHandler>()), Times.Exactly(this.jobHandlers.Count));
                this.jobWorkerBuilderStep3Mock.Verify(s => s.Name(handler.WorkerName), Times.Once);                
                this.jobWorkerBuilderStep3Mock.Verify(s => s.MaxJobsActive(handler.MaxJobsActive.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.Timeout(handler.Timeout.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.PollInterval(handler.PollInterval.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.PollingTimeout(handler.PollingTimeout.Value), Times.Once);                
                this.jobWorkerBuilderStep3Mock.Verify(s => s.FetchVariables(handler.FetchVariabeles), Times.Once);
            });
        }

        [Fact]
        public async Task DefaultsAreSetWhenHandlerPropertiesAreNull()
        {
            jobHandlers.Clear();
            jobHandlers.Add(new JobHandlerReference(
                typeof(JobHandlerA)
                    .GetMethods()
                    .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                    .First(),
                ServiceLifetime.Scoped,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            ));
            var expected = this.zeebeWorkerOptionsMock.Object;

            var service = Create();
            await service.StartAsync(cancellationToken);
            
            this.jobWorkerBuilderStep3Mock.Verify(s => s.MaxJobsActive(expected.MaxJobsActive), Times.Once);  
            this.jobWorkerBuilderStep3Mock.Verify(s => s.Timeout(expected.Timeout), Times.Once);
            this.jobWorkerBuilderStep3Mock.Verify(s => s.PollInterval(expected.PollInterval), Times.Once);
            this.jobWorkerBuilderStep3Mock.Verify(s => s.PollingTimeout(expected.PollingTimeout), Times.Once);
        }

        [Fact]
        public async Task HandlerDelegateExecutesTheCorrectJobHandlerMethodWhenExecuted() {
            var handlers = new List<AsyncJobHandler>();
            var jobs = new List<IJob>();

            this.jobWorkerBuilderStep2Mock.Setup(m => m.Handler(It.IsAny<AsyncJobHandler>()))
                .Returns(jobWorkerBuilderStep3Mock.Object)
                .Callback<AsyncJobHandler>(h => handlers.Add(h));

            this.handleJobDelegateMock.Setup(d => d.Invoke(It.IsAny<IJobClient>(), It.IsAny<IJob>(), It.IsAny<CancellationToken>()))
                .Callback<IJobClient, IJob, CancellationToken>((jobClient, job, cancellationToken) => {
                    Assert.NotNull(jobClient);
                    Assert.NotNull(job);

                    Assert.Equal(this.zeebeClientMock.Object, jobClient);
                    Assert.Equal(this.cancellationToken, cancellationToken);
                    jobs.Add(job);
                });

            var service = Create();
            await service.StartAsync(cancellationToken);

            Assert.Equal(this.jobHandlers.Count, handlers.Count);
            
            var jobMock = new Mock<IJob>();
            var job = jobMock.Object;

            Task.WaitAll(handlers.Select(h => h(this.zeebeClientMock.Object, job)).ToArray());

            this.handleJobDelegateMock.Verify(d => d.Invoke(It.IsAny<IJobClient>(), It.IsAny<IJob>(), It.IsAny<CancellationToken>()), Times.Exactly(this.jobHandlers.Count));
            Assert.Equal(jobs.Count, this.jobHandlers.Count);

            (new Type[] { typeof(JobA), typeof(JobB), typeof(JobC) })
                .All(t => jobs.Where(j => j.GetType().Equals(t)).Any());
        }

        [Fact]
        public async Task ExceptionIsRaisedWhenDelegateThrowsException() {
            var expected = new SystemException();

            var handlers = new List<AsyncJobHandler>();
            
            this.jobWorkerBuilderStep2Mock.Setup(m => m.Handler(It.IsAny<AsyncJobHandler>()))
                .Returns(jobWorkerBuilderStep3Mock.Object)
                .Callback<AsyncJobHandler>(h => handlers.Add(h));

            this.handleJobDelegateMock.Setup(d => d.Invoke(It.IsAny<IJobClient>(), It.IsAny<IJob>(), It.IsAny<CancellationToken>()))
                .Throws(expected);

            var service = Create();
            await service.StartAsync(cancellationToken);

            var jobMock = new Mock<IJob>();
            var job = jobMock.Object;

            var tasks = handlers.Select(h => h(this.zeebeClientMock.Object, job)).ToArray();

            try {
                Task.WaitAll(tasks);
            }
            catch { }

            Assert.Equal(this.jobHandlers.Count,  tasks.Where(t => t.IsFaulted && t.Exception.InnerException.InnerException.Equals(expected)).Count());
        }

        [Fact]
        public async Task AllWorkersAreDisposedWhenStopAsyncIsExecuted() 
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            await service.StopAsync(cancellationToken);

            this.jobWorkerMock.Verify(j => j.Dispose(), Times.Exactly(this.jobHandlers.Count));
        }

        [Fact]
        public async Task AllWorkersAreDisposedWhenServiceIsDisposed() 
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            service.Dispose();

            this.jobWorkerMock.Verify(j => j.Dispose(), Times.Exactly(this.jobHandlers.Count));
        }

        #region Prepare

        public ZeebeHostedServiceTests()
        {            
            this.cancellationToken = new CancellationToken();
            this.jobHandlers = new List<IJobHandlerReference>() {
               CreateJobHandlerReference(
                    typeof(JobHandlerA)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                        .First()
               ), 
               CreateJobHandlerReference(
                    typeof(JobHandlerA)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                        .ToArray()[1]
               ),
               CreateJobHandlerReference(
                    typeof(JobHandlerB)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                        .First()
               )
            };

            this.handleJobDelegateMock = new Mock<HandleJobDelegate>();
            this.serviceProviderMock = CreateIServiceProviderMock(this.handleJobDelegateMock);

            this.jobWorkerMock = CreateIJobWorkerMock();
            this.jobWorkerBuilderStep3Mock = CreateIJobWorkerBuilderStep3Mock(this.jobWorkerMock);
            this.jobWorkerBuilderStep2Mock = CreateIJobWorkerBuilderStep2Mock(this.jobWorkerBuilderStep3Mock);
            this.jobWorkerBuilderStep1Mock = CreateIJobWorkerBuilderStep1Mock(this.jobWorkerBuilderStep2Mock);
            this.zeebeClientMock = CreateIZeebeClientMock(this.jobWorkerBuilderStep1Mock);

            this.jobHandlerProviderMock = CreateIJobHandlerProviderMock();
            
            this.zeebeWorkerOptionsMock = CreateZeebeWorkerOptionsMock();
            this.zeebeClientBootstrapOptionsMock = CreateZeebeClientBootstrapOptionsMock(this.zeebeWorkerOptionsMock);
            this.optionsMock = CreateOptionsMock(this.zeebeClientBootstrapOptionsMock);
            
            this.loggerMock = new Mock<ILogger<ZeebeHostedService>>();
        }

        private ZeebeHostedService Create() 
        {
             return new ZeebeHostedService(
                this.serviceProviderMock.Object, 
                this.zeebeClientMock.Object, 
                this.jobHandlerProviderMock.Object,
                this.optionsMock.Object,
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

        private static Mock<IZeebeClient> CreateIZeebeClientMock(Mock<IJobWorkerBuilderStep1> step1Mock)
        {
            var mock = new Mock<IZeebeClient>();

            mock.Setup(c => c.NewWorker()).Returns(step1Mock.Object);

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

        private Mock<IJobHandlerProvider> CreateIJobHandlerProviderMock(Mock<IJobHandler<JobA>> jobHandlerMock = null)
        {
            var mock = new Mock<IJobHandlerProvider>();            

            mock.SetupGet(p => p.JobHandlers).Returns(this.jobHandlers);

            return mock;
        }

        private static Mock<IOptions<ZeebeClientBootstrapOptions>> CreateOptionsMock(Mock<ZeebeClientBootstrapOptions> zeebeClientBootstrapOptionsMock)
        {
            var mock = new Mock<IOptions<ZeebeClientBootstrapOptions>>();
            
            mock.SetupGet(m => m.Value).Returns(zeebeClientBootstrapOptionsMock.Object);

            return mock;
        }

        private static Mock<ZeebeClientBootstrapOptions> CreateZeebeClientBootstrapOptionsMock(Mock<WorkerOptions> workerOptionsMock)
        {
            var mock = new Mock<ZeebeClientBootstrapOptions>();

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

            return mock;
        }

        private static IJobHandlerReference CreateJobHandlerReference(MethodInfo handler) 
        {
            var random = new Random();
            
            return  new JobHandlerReference(
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

        #endregion
    }
}