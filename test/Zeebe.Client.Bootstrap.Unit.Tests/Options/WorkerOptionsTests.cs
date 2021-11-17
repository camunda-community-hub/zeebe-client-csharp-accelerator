using System;
using Xunit;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Options
{
    public class WorkerOptionsTests 
    {
        private readonly int maxJobsActive;
        private readonly long timeout;
        private readonly long pollInterval;
        private readonly long pollingTimeout;
        private readonly long retryTimeout;
        public readonly string name;

        [Fact]
        public void TimeoutTimeSpanMatchesTimeoutInMillisecondsWhenCreated()
        {   
            var actual = Create();

            Assert.Equal(this.timeout, actual.Timeout.TotalMilliseconds);
        }

        [Fact]
        public void PollingTimeoutTimeSpanMatchesPollingTimeoutnMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(this.pollingTimeout, actual.PollingTimeout.TotalMilliseconds);
        }

        [Fact]
        public void PollIntervalTimeSpanMatchesPollIntervalInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(this.pollInterval, actual.PollInterval.TotalMilliseconds);
        }

        [Fact]
        public void RetryTimeoutTimeSpanMatchesRetryTimeoutInMillisecondsWhenCreated()
        {   
            var actual = Create();
            
            Assert.Equal(this.retryTimeout, actual.RetryTimeout.TotalMilliseconds);
        }

        public WorkerOptionsTests()
        {
            var random = new Random();

            this.maxJobsActive = random.Next(1, int.MaxValue);
            this.timeout = (long)random.Next(1, int.MaxValue);
            this.pollInterval = (long)random.Next(1, int.MaxValue);
            this.pollingTimeout = (long)random.Next(1, int.MaxValue);
            this.retryTimeout = (long)random.Next(1, int.MaxValue);
            this.name = Guid.NewGuid().ToString();
        }

        private WorkerOptions Create()
        {
            var options = new WorkerOptions
            {
                MaxJobsActive = this.maxJobsActive,
                TimeoutInMilliseconds = this.timeout,
                PollingTimeoutInMilliseconds = this.pollingTimeout,
                PollIntervalInMilliseconds = this.pollInterval,
                RetryTimeoutInMilliseconds = this.retryTimeout,
            Name = this.name
            };

            return options;
        }
    }
}