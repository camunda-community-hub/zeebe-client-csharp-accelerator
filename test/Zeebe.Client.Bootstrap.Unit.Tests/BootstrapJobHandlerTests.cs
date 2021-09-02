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
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class BootstrapJobHandlerTests 
    {
        private readonly CancellationToken cancellationToken;
        private readonly List<IJobHandlerInfo> jobHandlers;
        private readonly Mock<HandleJobDelegate> handleJobDelegateMock;
        private readonly Mock<IZeebeClient> zeebeClientMock;
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IJobWorker> jobWorkerMock;
        private readonly Mock<IJobWorkerBuilderStep3> jobWorkerBuilderStep3Mock;
        private readonly Mock<IJobWorkerBuilderStep2> jobWorkerBuilderStep2Mock;
        private readonly Mock<IJobWorkerBuilderStep1> jobWorkerBuilderStep1Mock;
        private readonly Mock<IJobHandlerProvider> jobHandlerProviderMock;
        private readonly Mock<IZeebeVariablesSerializer> serializerMock;
        private readonly Mock<ILogger<BootstrapJobHandler>> loggerMock;

        [Fact]
        public void ThrowsArgumentNullExceptionWhenServiceProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serviceProvider", () => new BootstrapJobHandler(null, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.serializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenClientIsNull() 
        {
            Assert.Throws<ArgumentNullException>("client", () => new BootstrapJobHandler(this.serviceProviderMock.Object, null, this.jobHandlerProviderMock.Object, this.serializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenJobHandlerProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("jobHandlerProvider", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, null, this.serializerMock.Object, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenSerializerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("serializer", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, null, this.loggerMock.Object));
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenLoggerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("logger", () => new BootstrapJobHandler(this.serviceProviderMock.Object, this.zeebeClientMock.Object, this.jobHandlerProviderMock.Object, this.serializerMock.Object, null));
        }
        
        /*[Fact]
        public async Task HandlerDelegateExecutesTheCorrectJobHandlerMethodWhenExecuted() {
            var handlers = new List<BootstrapJobHandler>();
            var jobs = new List<IJob>();

            this.jobWorkerBuilderStep2Mock.Setup(m => m.Handler(It.IsAny<BootstrapJobHandler>()))
                .Returns(jobWorkerBuilderStep3Mock.Object)
                .Callback<BootstrapJobHandler>(h => handlers.Add(h));

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

            var handlers = new List<BootstrapJobHandler>();
            
            this.jobWorkerBuilderStep2Mock.Setup(m => m.Handler(It.IsAny<BootstrapJobHandler>()))
                .Returns(jobWorkerBuilderStep3Mock.Object)
                .Callback<BootstrapJobHandler>(h => handlers.Add(h));

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
        }*/

        #region Prepare

        public BootstrapJobHandlerTests()
        {            
            this.cancellationToken = new CancellationToken();
            this.jobHandlers = new List<IJobHandlerInfo>() {
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
            
            this.serializerMock = CreateSerializerMock();
            
            this.loggerMock = new Mock<ILogger<BootstrapJobHandler>>();
        }

        private BootstrapJobHandler Create() 
        {
             return new BootstrapJobHandler(
                this.serviceProviderMock.Object, 
                this.zeebeClientMock.Object, 
                this.jobHandlerProviderMock.Object,
                this.serializerMock.Object,
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

        private Mock<IJobHandlerProvider> CreateIJobHandlerProviderMock()
        {
            var mock = new Mock<IJobHandlerProvider>();            

            mock.SetupGet(p => p.JobHandlers).Returns(this.jobHandlers);

            return mock;
        }

        private static Mock<IZeebeVariablesSerializer> CreateSerializerMock()
        {
            var mock = new Mock<IZeebeVariablesSerializer>();

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

        #endregion
    }
}