using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;

namespace Zeebe.Client.Bootstrap.Unit.Tests
{
    public class JobHandlerProviderTests
    {
        [Fact]
        public void ThrowsArgumentNullExceptionWhenAssemblyProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("assemblyProvider", () => new JobHandlerProvider(null));
        }

        [Fact]
        public void AllJobHandlersAreFoundWhenCreated() {
            var actual = Handlers();
            var expected = 6;

            Assert.Equal(expected, actual.Count());
        }

        [Fact]
        public void WorkerNamePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.WorkerName);
            Assert.Contains(Meta.UNIT_TEST_PROJECT_NAME, actual);
            Assert.Contains("TestWorkerName", actual);
        }

        [Fact]
        public void JobTypePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.JobType);
            Assert.Contains(nameof(JobA), actual);
            Assert.Contains("TestJobType", actual);
            Assert.Contains(nameof(JobC), actual);
            Assert.DoesNotContain(nameof(JobB), actual);
        }

        [Fact]
        public void HandlerServiceLifeTimePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.HandlerServiceLifetime);
            Assert.Contains(ServiceLifetime.Scoped, actual);
            Assert.Contains(ServiceLifetime.Transient, actual);
        }

        [Fact]
        public void MaxJobsActivePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.MaxJobsActive);
            Assert.Contains(null, actual);
            Assert.Contains(int.MaxValue - 1, actual);
        }

        [Fact]
        public void TimeoutPropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.Timeout);
            Assert.Contains(null, actual);
            Assert.Contains(TimeSpan.FromMilliseconds(int.MaxValue - 2), actual);
        }

        [Fact]
        public void PollIntervalPropertyIsSetCorrectlyWhenCreated()
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.PollInterval);
            Assert.Contains(null, actual);
            Assert.Contains(TimeSpan.FromMilliseconds(int.MaxValue - 4), actual);
        }

        [Fact]
        public void PollingTimeoutPropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.PollingTimeout);
            Assert.Contains(null, actual);
            Assert.Contains(TimeSpan.FromMilliseconds(int.MaxValue - 3), actual);
        }

        private static JobHandlerProvider Create()
        {
            return new JobHandlerProvider(new AssemblyProvider(Meta.UNIT_TEST_PROJECT_NAME));
        }

        private static IEnumerable<IJobHandlerInfo> Handlers()
        {
            var provider = Create();
            return provider.JobHandlers;
        }
    }
}
