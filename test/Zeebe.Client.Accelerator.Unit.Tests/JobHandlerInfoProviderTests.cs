using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Unit.Tests.Stubs;

namespace Zeebe.Client.Accelerator.Unit.Tests
{
    public class JobHandlerInfoProviderTests
    {
        private readonly Assembly assembly;
        public JobHandlerInfoProviderTests(ITestOutputHelper testOutputHelper)
        {
            this.assembly = typeof(JobHandlerInfoProviderTests).Assembly;
        }
 
        [Fact]
        public void ThrowsArgumentNullExceptionWhenAssemblyProviderIsNull() 
        {
            Assert.Throws<ArgumentNullException>("assemblies", () => new JobHandlerInfoProvider(null));
        }

        [Fact]
        public void AllJobHandlersAreFoundWhenCreated() {
            var actual = Handlers();
            var expected = 8;

            Assert.Equal(expected, actual.Count());
        }

        [Fact]
        public void WorkerNamePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.WorkerName);
            Assert.Contains(this.assembly.GetName().Name, actual);
            Assert.Contains("TestWorkerName", actual);
        }

        [Fact]
        public void JobTypePropertyIsSetCorrectlyWhenCreated() 
        {
            var handlers = Handlers();

            var actual = handlers.Select(h => h.JobType);
            Assert.Contains("TestJobType", actual);
            Assert.DoesNotContain(nameof(JobHandlerA), actual);
            Assert.Contains(nameof(JobHandlerC), actual);
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
        public void FetchVariablesPropertyIsSetCorrectlyWhenCreated() 
        {
            var expected = new string[] { "1", "2", "3", "4", "5" };
            var handlers = Handlers();

            var actual = handlers.Select(h => h.FetchVariabeles);
            Assert.Contains(new string[0], actual);
            Assert.Contains(expected, actual);
        }

        [Fact]
        public void FetchVariablesPropertyIsSetCorrectlyWhenGenericStateIsUsed() 
        {
            var expected = new string[] { "guid", "bool", "int", "dateTime", "string", "MY_double" };
            var handlers = Handlers();

            var actual = handlers.Select(h => h.FetchVariabeles);
            Assert.Contains(new string[0], actual);
            Assert.Contains(expected, actual);
            Assert.DoesNotContain(new string[]{ "double", "toBeIgnored" }, actual);
        }

        [Fact]
        public void FetchVariablesPropertyIsSetCorrectlyWhenTypedZeebeJobIsUsed()
        {
            var expected = new string[] { "zeebeGuid", "zeebeBool", "zeebeInt", "zeebeDateTime", "zeebeString", "zeebeDouble" };
            var handlers = Handlers();

            var actual = handlers.Select(h => h.FetchVariabeles);
            Assert.Contains(new string[0], actual);
            Assert.Contains(expected, actual);
        }

        [Fact]
        public void FetchVariablesPropertyIsSetCorrectlyWhenGenericStateAttributeIsUsed() 
        {
            var expected = new string[] { "var100", "var101", "variable102", "variable103", "differentThing104" };
            var handlers = Handlers();

            var actual = handlers.Select(h => h.FetchVariabeles);
            Assert.Contains(new string[0], actual);
            Assert.Contains(expected, actual);
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

        private JobHandlerInfoProvider Create()
        {
            return new JobHandlerInfoProvider(this.assembly);
        }

        private IEnumerable<IJobHandlerInfo> Handlers()
        {
            var provider = Create();
            return provider.JobHandlerInfoCollection;
        }
    }
}
