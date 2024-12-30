using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeebe.Client.Accelerator.Unit.Tests
{
    public class JobHandlerInfoTests
    {
        private readonly MethodInfo handler;
        private readonly ServiceLifetime handlerServiceLifetime;
        private readonly string jobType;
        private readonly string workerName;
        private readonly TimeSpan timeout;
        private readonly int maxJobsActive;
        private readonly byte handlerThreads;
        private readonly TimeSpan pollingTimeout;
        private readonly TimeSpan pollInterval;
        private string[] fetchVariabeles;
        private string[] tenantIds;

        [Fact]
        public void ThrowsArgumenNullExceptionWhenHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>("handler", () => new JobHandlerInfo(null, this.handlerServiceLifetime, this.jobType, this.workerName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenJobTypeIsNullOrEmptyOrWhiteSpace(string jobType)
        {
            Assert.Throws<ArgumentException>("jobType", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, jobType, this.workerName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWheWorkerNameIsNullOrEmptyOrWhiteSpace(string workerName)
        {
            Assert.Throws<ArgumentException>("workerName", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, workerName));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenMaxJobsActiveIsSmallerOrEqualThen0(int maxJobsActive)
        {
            Assert.Throws<ArgumentOutOfRangeException>("maxJobsActive", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, maxJobsActive, this.handlerThreads, this.timeout, this.pollInterval, this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        public void ThrowsArgumentOutOfRangeExceptionWhenHandlerThreadsActiveIs0(byte handlerThreads)
        {
            Assert.Throws<ArgumentOutOfRangeException>("handlerThreads", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, handlerThreads, this.timeout, this.pollInterval, this.pollingTimeout));
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenTimeoutIsSmallerOrEqualThen0(int timeout)
        {
            Assert.Throws<ArgumentOutOfRangeException>("timeout", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, this.handlerThreads, TimeSpan.FromMilliseconds(timeout), this.pollInterval, this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollIntervalIsSmallerOrEqualThen0(int pollInterval)
        {
            Assert.Throws<ArgumentOutOfRangeException>("pollInterval", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, this.handlerThreads, this.timeout, TimeSpan.FromMilliseconds(pollInterval), this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollingTimeoutIsSmallerOrEqualThen0(int pollingTimeout)
        {
            Assert.Throws<ArgumentOutOfRangeException>("pollingTimeout", () => new JobHandlerInfo(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, this.handlerThreads, this.timeout, this.pollInterval, TimeSpan.FromMilliseconds(pollingTimeout)));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {
            var actual = Create();
            Assert.NotNull(actual);
            Assert.Equal(this.handler, actual.Handler);
            Assert.Equal(this.jobType, actual.JobType);
            Assert.Equal(this.workerName, actual.WorkerName);
            Assert.Equal(this.maxJobsActive, actual.MaxJobsActive);
            Assert.Equal(this.handlerThreads, actual.HandlerThreads);
            Assert.Equal(this.timeout, actual.Timeout);
            Assert.Equal(this.pollInterval, actual.PollInterval);
            Assert.Equal(this.pollingTimeout, actual.PollingTimeout);
            Assert.Equal(this.fetchVariabeles, actual.FetchVariabeles);
            Assert.Equal(this.tenantIds, actual.TenantIds);
        }

        [Fact]
        public void EmptyStringArrayIsCreatedWhenFetchVariabelesIsNull()
        {
            this.fetchVariabeles = null;
            var actual = Create();
            Assert.Equal(new string[0], actual.FetchVariabeles);
        }

        [Fact]
        public void EmptyStringArrayIsCreatedWhenTenantIdsIsNull()
        {
            this.tenantIds = null;
            var actual = Create();
            Assert.Equal(new string[0], actual.TenantIds);
        }

        public JobHandlerInfoTests()
        {
            Expression<Func<int, string>> expression = i => i.ToString();
            var handler = ((MethodCallExpression)expression.Body).Method;

            var random = new Random();
            this.handler = handler;
            this.handlerServiceLifetime = ServiceLifetime.Scoped;
            this.jobType = Guid.NewGuid().ToString();
            this.workerName = Guid.NewGuid().ToString();
            this.timeout = TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue));
            this.maxJobsActive = random.Next(1, int.MaxValue);
            this.handlerThreads = Convert.ToByte(random.Next(1, 255));
            this.pollingTimeout = TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue));
            this.pollInterval = TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue));
            this.fetchVariabeles = new string[] {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            this.tenantIds = new string[] {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };
        }

        private JobHandlerInfo Create()
        {
            return new JobHandlerInfo
            (
                this.handler,
                this.handlerServiceLifetime,
                this.jobType,
                this.workerName,
                this.maxJobsActive,
                this.handlerThreads,
                this.timeout,
                this.pollInterval,
                this.pollingTimeout,
                this.fetchVariabeles,
                tenantIds: this.tenantIds
             );
        }
    }
}
