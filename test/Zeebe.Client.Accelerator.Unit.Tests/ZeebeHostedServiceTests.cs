using System;
using System.Linq;
using Xunit;
using Zeebe.Client.Accelerator.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Unit.Tests.Stubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Accelerator.Options;
using Microsoft.Extensions.Options;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;

namespace Zeebe.Client.Accelerator.Unit.Tests
{
    public class ZeebeHostedServiceTests 
    {
        private readonly CancellationToken cancellationToken;
        private readonly List<IJobHandlerInfo> jobHandlerInfoCollection;
        private readonly Mock<HandleJobDelegate> handleJobDelegateMock;
        private readonly Mock<IZeebeClient> zeebeClientMock;
        private readonly Mock<IBootstrapJobHandler> bootstrapJobHandlerMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IServiceScope> serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private readonly Mock<IJobWorker> jobWorkerMock;
        private readonly Mock<IJobWorkerBuilderStep3> jobWorkerBuilderStep3Mock;
        private readonly Mock<IJobWorkerBuilderStep2> jobWorkerBuilderStep2Mock;
        private readonly Mock<IJobWorkerBuilderStep1> jobWorkerBuilderStep1Mock;
        private readonly Mock<IJobHandlerInfoProvider> jobHandlerInfoProviderMock;
        private readonly Mock<IOptions<ZeebeClientAcceleratorOptions>> optionsMock;
        private readonly Mock<ZeebeClientAcceleratorOptions> zeebeClientBootstrapOptionsMock;
        private readonly Mock<WorkerOptions> zeebeWorkerOptionsMock;
        private readonly Mock<ILogger<ZeebeHostedService>> loggerMock;

        [Fact]
        public void ThrowsArgumentNullExceptionWhenBootstrapJobHandlerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serviceScopeFactory", () => new ZeebeHostedService(null, this.jobHandlerInfoProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobHandlerInfoProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("jobHandlerInfoProvider", () => new ZeebeHostedService(this.serviceScopeFactoryMock.Object, null, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsIsNull() 
        {
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceScopeFactoryMock.Object, this.jobHandlerInfoProviderMock.Object, null, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueIsNull() 
        {
            this.optionsMock.SetupGet(m => m.Value).Returns((ZeebeClientAcceleratorOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceScopeFactoryMock.Object, this.jobHandlerInfoProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenOptionsValueWorkerIsNull() 
        {
            this.zeebeClientBootstrapOptionsMock.SetupGet(m => m.Worker).Returns((WorkerOptions)null);
            Assert.Throws<ArgumentNullException>("options", () => new ZeebeHostedService(this.serviceScopeFactoryMock.Object, this.jobHandlerInfoProviderMock.Object, this.optionsMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenLoggerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("logger", () => new ZeebeHostedService(this.serviceScopeFactoryMock.Object, this.jobHandlerInfoProviderMock.Object, this.optionsMock.Object, null));
        }

        [Fact]
        public async Task WorkersAreCreatedWhenStartIsExecuted()
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            
            this.zeebeClientMock.Verify(c => c.NewWorker(), Times.Exactly(this.jobHandlerInfoCollection.Count));
        }

        [Fact]
        public async Task WorkerPropertiesAreSetWhenCreated()
        {
            zeebeWorkerOptionsMock.Setup(m => m.Name).Returns((string)null);

            var service = Create();
            await service.StartAsync(cancellationToken);
            
            this.jobHandlerInfoCollection.ForEach(info => {
                this.jobWorkerBuilderStep1Mock.Verify(c => c.JobType(info.JobType), Times.Once);
                this.jobWorkerBuilderStep2Mock.Verify(c => c.Handler(It.IsAny<AsyncJobHandler>()), Times.Exactly(this.jobHandlerInfoCollection.Count));
                this.jobWorkerBuilderStep3Mock.Verify(s => s.Name(info.WorkerName), Times.Once);                
                this.jobWorkerBuilderStep3Mock.Verify(s => s.MaxJobsActive(info.MaxJobsActive.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.Timeout(info.Timeout.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.PollInterval(info.PollInterval.Value), Times.Once);
                this.jobWorkerBuilderStep3Mock.Verify(s => s.PollingTimeout(info.PollingTimeout.Value), Times.Once);                
                this.jobWorkerBuilderStep3Mock.Verify(s => s.FetchVariables(info.FetchVariabeles), Times.Once);
            });
        }

        [Fact]
        public async Task DefaultsAreSetWhenHandlerPropertiesAreNull()
        {
            jobHandlerInfoCollection.Clear();
            jobHandlerInfoCollection.Add(new JobHandlerInfo(
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

            this.handleJobDelegateMock.Setup(d => d.Invoke(It.IsAny<IJob>(), It.IsAny<CancellationToken>()))
                .Callback<IJob, CancellationToken>((job, cancellationToken) => {
                    Assert.NotNull(job);
                    jobs.Add(job);
                });

            var service = Create();
            await service.StartAsync(cancellationToken);

            Assert.Equal(this.jobHandlerInfoCollection.Count, handlers.Count);
            
            var jobMock = new Mock<IJob>();
            var job = jobMock.Object;
            
            this.serviceScopeFactoryMock.Invocations.Clear();
            this.serviceScopeMock.Invocations.Clear();

            Task.WaitAll(handlers.Select(h => h(this.zeebeClientMock.Object, job)).ToArray());

            this.handleJobDelegateMock.Verify(d => d.Invoke(It.IsAny<IJob>(), It.IsAny<CancellationToken>()), Times.Exactly(this.jobHandlerInfoCollection.Count));
            Assert.Equal(jobs.Count, this.jobHandlerInfoCollection.Count);

            (new Type[] { typeof(ZeebeJob), typeof(JobG), typeof(JobH) })
                .All(t => jobs.Where(j => j.GetType().Equals(t)).Any());
            
            this.serviceScopeFactoryMock.Verify(m => m.CreateScope(), Times.Exactly(jobs.Count), "ServiceScope has not been created for each job handling.");
            this.serviceScopeMock.Verify(m => m.Dispose(), Times.Exactly(jobs.Count), "ServiceScope has not been disposed for each job handling.");
        }

        [Fact]
        public async Task ExceptionIsRaisedWhenDelegateThrowsException() {
            var expected = new SystemException();
            var handlers = new List<AsyncJobHandler>();
            
            this.jobWorkerBuilderStep2Mock.Setup(m => m.Handler(It.IsAny<AsyncJobHandler>()))
                .Returns(jobWorkerBuilderStep3Mock.Object)
                .Callback<AsyncJobHandler>(h => handlers.Add(h));

            this.handleJobDelegateMock.Setup(d => d.Invoke(It.IsAny<IJob>(), It.IsAny<CancellationToken>()))
                .Throws(expected);

            var service = Create();
            await service.StartAsync(cancellationToken);

            var jobMock = new Mock<IJob>();
            var job = jobMock.Object;

            var tasks = handlers.Select(h => h(this.zeebeClientMock.Object, job)).ToArray();

            try 
            {
                Task.WaitAll(tasks);
            }
            catch { }

            Assert.Equal(this.jobHandlerInfoCollection.Count,  tasks.Where(t => t.IsFaulted && t.Exception.InnerException.Equals(expected)).Count());
        }

        [Fact]
        public async Task AllIsStopedWhenStopAsyncIsExecuted() 
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            await service.StopAsync(cancellationToken);

            this.jobWorkerMock.Verify(j => j.Dispose(), Times.Exactly(this.jobHandlerInfoCollection.Count));
        }

        [Fact]
        public async Task AllIsDisposedWhenServiceIsDisposed() 
        {
            var service = Create();
            await service.StartAsync(cancellationToken);
            service.Dispose();

            this.jobWorkerMock.Verify(j => j.Dispose(), Times.Exactly(this.jobHandlerInfoCollection.Count));
            this.serviceScopeMock.Verify(j => j.Dispose(), Times.Once);
        }

        #region Prepare

        public ZeebeHostedServiceTests()
        {            
            this.cancellationToken = new CancellationToken();
            this.jobHandlerInfoCollection = new List<IJobHandlerInfo>() {
               CreateJobHandlerInfo(
                    typeof(JobHandlerA)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerA.HandleJob)))
                        .First()
               ), 
               CreateJobHandlerInfo(
                    typeof(JobHandlerB)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerB.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerC)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerC.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerD)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerD.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerE)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerE.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerF)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerF.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerG)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerG.HandleJob)))
                        .First()
               ),
               CreateJobHandlerInfo(
                    typeof(JobHandlerH)
                        .GetMethods()
                        .Where(m => m.Name.Equals(nameof(JobHandlerH.HandleJob)))
                        .First()
               ),
            };

            this.handleJobDelegateMock = new Mock<HandleJobDelegate>();

            this.jobWorkerMock = CreateIJobWorkerMock();
            this.jobWorkerBuilderStep3Mock = CreateIJobWorkerBuilderStep3Mock(this.jobWorkerMock);
            this.jobWorkerBuilderStep2Mock = CreateIJobWorkerBuilderStep2Mock(this.jobWorkerBuilderStep3Mock);
            this.jobWorkerBuilderStep1Mock = CreateIJobWorkerBuilderStep1Mock(this.jobWorkerBuilderStep2Mock);            
            this.zeebeClientMock = CreateIZeebeClientMock(this.jobWorkerBuilderStep1Mock);

            this.bootstrapJobHandlerMock = CreateBootstrapJobHandlerMock(this.handleJobDelegateMock);

            this.jobHandlerInfoProviderMock = CreateIJobHandlerInfoProviderMock();
            
            this.zeebeWorkerOptionsMock = CreateZeebeWorkerOptionsMock();
            this.zeebeClientBootstrapOptionsMock = CreateZeebeClientBootstrapOptionsMock(this.zeebeWorkerOptionsMock);
            this.optionsMock = CreateOptionsMock(this.zeebeClientBootstrapOptionsMock);
            
            this.loggerMock = new Mock<ILogger<ZeebeHostedService>>();

            this.serviceProviderMock = CreateServiceProviderMock(this.zeebeClientMock, this.jobHandlerInfoProviderMock);
            this.serviceScopeMock = CreateServiceScopeMock(this.serviceProviderMock);
            this.serviceScopeFactoryMock = CreateServiceScopeFactoryMock(this.serviceScopeMock);
        }

        private ZeebeHostedService Create() 
        {
             return new ZeebeHostedService(
                this.serviceScopeFactoryMock.Object,
                this.jobHandlerInfoProviderMock.Object,
                this.optionsMock.Object,
                this.loggerMock.Object
            );
        }

        private Mock<IServiceScope> CreateServiceScopeMock(Mock<IServiceProvider> serviceProviderMock)
        {
            var mock = new Mock<IServiceScope>();

            mock.Setup(m => m.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            return mock;
        }

        private Mock<IServiceScopeFactory> CreateServiceScopeFactoryMock(Mock<IServiceScope> serviceScopeMock)
        {
            var mock = new Mock<IServiceScopeFactory>();

            mock.Setup(m => m.CreateScope())
                .Returns(serviceScopeMock.Object);

            return mock;
        }

        private Mock<IServiceProvider> CreateServiceProviderMock(Mock<IZeebeClient> zeebeClientMock, Mock<IJobHandlerInfoProvider> jobHandlerInfoProviderMock)
        {
            var mock = new Mock<IServiceProvider>();

            mock.Setup(m => m.GetService(It.Is<Type>(t => t.Equals(typeof(IZeebeClient)))))
                .Returns(zeebeClientMock.Object);

            mock.Setup(m => m.GetService(It.Is<Type>(t => t.Equals(typeof(IBootstrapJobHandler)))))
                .Returns(bootstrapJobHandlerMock.Object);

            return mock;
        }

        private static Mock<IBootstrapJobHandler> CreateBootstrapJobHandlerMock(Mock<HandleJobDelegate> handleJobDelegateMock)
        {
            var mock =  new Mock<IBootstrapJobHandler>();

            mock.Setup(m => m.HandleJob(It.IsAny<IJobClient>(), It.IsAny<IJob>(), It.IsAny<CancellationToken>()))
                .Returns<IJobClient, IJob, CancellationToken>((jobClient, job, cancellationToken) => {
                    try
                    {
                        handleJobDelegateMock.Object.Invoke(job, cancellationToken);
                        return Task.CompletedTask;                
                    }
                    catch(Exception ex) 
                    {
                        return Task.FromException(ex);
                    }
                });
            
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

        private Mock<IJobHandlerInfoProvider> CreateIJobHandlerInfoProviderMock()
        {
            var mock = new Mock<IJobHandlerInfoProvider>();            

            mock.SetupGet(p => p.JobHandlerInfoCollection).Returns(this.jobHandlerInfoCollection);

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

        private static IJobHandlerInfo CreateJobHandlerInfo(MethodInfo handler) 
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

        #endregion
    }
}