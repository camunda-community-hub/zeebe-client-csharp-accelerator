using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
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
            var provider = new JobHandlerProvider(new AssemblyProvider(Meta.UNIT_TEST_PROJECT_NAME));
            
            Assert.Equal(3, provider.JobHandlers.Count());
        }

        [Fact]
        public void AllJobHandlerPropertiesAreSetCorrectlyWhenCreated() {
            var provider = new JobHandlerProvider(new AssemblyProvider(Meta.UNIT_TEST_PROJECT_NAME));
            var handlers = provider.JobHandlers;

            var workerNames = handlers.Select(h => h.WorkerName);
            Assert.Contains(Meta.UNIT_TEST_PROJECT_NAME, workerNames);
            Assert.Contains("TestWorkerName", workerNames);

            var jobTypes = handlers.Select(h => h.JobType);
            Assert.Contains(nameof(JobA), jobTypes);
            Assert.Contains("TestJobType", jobTypes);
            Assert.Contains(nameof(JobC), jobTypes);            
            Assert.DoesNotContain(nameof(JobB), jobTypes);

            var handlerServiceLifeTimes = handlers.Select(h => h.HandlerServiceLifetime);
            Assert.Contains(ServiceLifetime.Scoped, handlerServiceLifeTimes);
            Assert.Contains(ServiceLifetime.Transient, handlerServiceLifeTimes);
        }
    }
}
