using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class JobHandlerReferenceTests
    {
        private readonly MethodInfo handler;
        private readonly ServiceLifetime handlerServiceLifetime;
        private readonly string jobType;
        private readonly string workerName;
        private readonly TimeSpan timeout;
        private readonly int maxJobsActive;
        private readonly TimeSpan pollingTimeout;
        private readonly TimeSpan pollInterval;
        private string[] fetchVariabeles;

        [Fact]
        public void ThrowsArgumenNullExceptionWhenHandlerIsNull() 
        {
            Assert.Throws<ArgumentNullException>("handler", () => new JobHandlerReference(null, this.handlerServiceLifetime, this.jobType, this.workerName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenJobTypeIsNullOrEmptyOrWhiteSpace(string jobType)
        {
            Assert.Throws<ArgumentException>("jobType", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, jobType, this.workerName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWheWorkerNameIsNullOrEmptyOrWhiteSpace(string workerName)
        {
            Assert.Throws<ArgumentException>("workerName", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, this.jobType, workerName));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenMaxJobsActiveIsSmallerOrEqualThen0(int maxJobsActive) 
        {
            Assert.Throws<ArgumentOutOfRangeException>("maxJobsActive", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, maxJobsActive, this.timeout, this.pollInterval, this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenTimeoutIsSmallerOrEqualThen0(int timeout) 
        {
            Assert.Throws<ArgumentOutOfRangeException>("timeout", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, TimeSpan.FromMilliseconds(timeout), this.pollInterval, this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollIntervalIsSmallerOrEqualThen0(int pollInterval) 
        {
            Assert.Throws<ArgumentOutOfRangeException>("pollInterval", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, this.timeout, TimeSpan.FromMilliseconds(pollInterval), this.pollingTimeout));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollingTimeoutIsSmallerOrEqualThen0(int pollingTimeout) 
        {
            Assert.Throws<ArgumentOutOfRangeException>("pollingTimeout", () => new JobHandlerReference(this.handler, this.handlerServiceLifetime, this.jobType, this.workerName, this.maxJobsActive, this.timeout, this.pollInterval, TimeSpan.FromMilliseconds(pollingTimeout)));
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
            Assert.Equal(this.timeout, actual.Timeout);
            Assert.Equal(this.pollInterval, actual.PollInterval);
            Assert.Equal(this.pollingTimeout, actual.PollingTimeout);
            Assert.Equal(this.fetchVariabeles, actual.FetchVariabeles);
        }

        [Fact]
        public void EmptyStringArrayIsCreatedWhenFetchVariabelesIsNull()
        {
            this.fetchVariabeles = null;
            var actual = Create();
            Assert.Equal(new string[0], actual.FetchVariabeles);
        }

        public JobHandlerReferenceTests()
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
            this.pollingTimeout = TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue));
            this.pollInterval = TimeSpan.FromMilliseconds(random.Next(1, int.MaxValue));
            this.fetchVariabeles = new string[] { 
                Guid.NewGuid().ToString(), 
                Guid.NewGuid().ToString() 
            };
        }

        private JobHandlerReference Create() 
        {
            return new JobHandlerReference
            (
                this.handler,
                this.handlerServiceLifetime,
                this.jobType,
                this.workerName,
                this.maxJobsActive,
                this.timeout,
                this.pollInterval,
                this.pollingTimeout,
                this.fetchVariabeles
            );
        }     
    }
}
