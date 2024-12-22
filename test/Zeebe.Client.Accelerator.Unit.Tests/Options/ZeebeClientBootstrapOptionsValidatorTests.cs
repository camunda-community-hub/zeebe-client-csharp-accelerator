using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Zeebe.Client.Accelerator.Options;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Options
{
    public class ZeebeClientBootstrapOptionsValidatorTests
    {
        private readonly Mock<WorkerOptions> zeebeWorkerOptionsMock;
        private readonly Mock<ClientOptions> zeebeClientOptionsMock;

        public Mock<ZeebeClientAcceleratorOptions> zeebeClientBootstrapOptionsMock { get; }

        private readonly Mock<IOptions<ZeebeClientAcceleratorOptions>> optionsMock;

        [Fact]
        public void ThrowsNoExceptionWhenOptionsIsValid()
        {
            Exception expected = null;
            try
            {
                Validate();
            }
            catch(Exception ex) 
            {
                expected = ex;   
            }

            Assert.Null(expected);
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenClientIsNull()
        {
            this.zeebeClientBootstrapOptionsMock.SetupGet(m => m.Client).Returns((ClientOptions)null);
            AssertSingleArgumentException<ArgumentNullException>("Client");
        }

        [Fact]
        public void ThrowsArgumentNullExceptionWhenWorkertIsNull()
        {
            this.zeebeClientBootstrapOptionsMock.SetupGet(m => m.Worker).Returns((WorkerOptions)null);
            AssertSingleArgumentException<ArgumentNullException>("Worker");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenMaxJobsActiveIsSmallerOrEqualThen0(int maxJobsActive)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.MaxJobsActive).Returns(maxJobsActive);
            AssertSingleArgumentException<ArgumentOutOfRangeException>("Worker.MaxJobsActive");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenTimeoutIsSmallerOrEqualThen0(int timeout)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.Timeout).Returns(TimeSpan.FromMilliseconds(timeout));
            AssertSingleArgumentException<ArgumentOutOfRangeException>("Worker.Timeout");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollIntervalIsSmallerOrEqualThen0(int pollInterval)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.PollInterval).Returns(TimeSpan.FromMilliseconds(pollInterval));
            AssertSingleArgumentException<ArgumentOutOfRangeException>("Worker.PollInterval");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenPollingTimeoutIsSmallerOrEqualThen0(int pollingTimeout)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.PollingTimeout).Returns(TimeSpan.FromMilliseconds(pollingTimeout));
            AssertSingleArgumentException<ArgumentOutOfRangeException>("Worker.PollingTimeout");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentOutOfRangeExceptionWhenRetryTimeoutIsSmallerOrEqualThen0(int retryTimeout)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.RetryTimeout).Returns(TimeSpan.FromMilliseconds(retryTimeout));
            AssertSingleArgumentException<ArgumentOutOfRangeException>("Worker.RetryTimeout");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenNameIsEmpty(string name)
        {
            this.zeebeWorkerOptionsMock.SetupGet(m => m.Name).Returns(name);
            AssertSingleArgumentException<ArgumentException>("Worker.Name");
        }

        #region Prepare

        public ZeebeClientBootstrapOptionsValidatorTests()
        {
            this.zeebeWorkerOptionsMock = CreateZeebeWorkerOptionsMock();
            this.zeebeClientOptionsMock = CreateZeebeClientOptionsMock();
            this.zeebeClientBootstrapOptionsMock = CreateZeebeClientBootstrapOptionsMock(this.zeebeClientOptionsMock, this.zeebeWorkerOptionsMock);
            this.optionsMock = CreateOptionsMock(this.zeebeClientBootstrapOptionsMock);
        }

        private void Validate()
        {
            new ZeebeClientAcceleratorOptionsValidator().Validate(zeebeClientBootstrapOptionsMock.Object);
        }

        private void AssertSingleArgumentException<T>(string paramName)
            where T : ArgumentException
        {
            var exceptions = Exceptions() as AggregateException;
            Assert.NotNull(exceptions);
            Assert.Single(exceptions.InnerExceptions);
            var argumentException = exceptions.InnerExceptions.First() as T;
            Assert.NotNull(argumentException);
            Assert.Equal(paramName, argumentException.ParamName);
        }

        private Exception Exceptions()
        {
            try
            {
                Validate();
            }
            catch(Exception ex)
            {
                return ex;
            }

            Assert.False(false, "Expected exception has not been thrown.");
            return null;
        }

        private static Mock<IOptions<ZeebeClientAcceleratorOptions>> CreateOptionsMock(Mock<ZeebeClientAcceleratorOptions> zeebeClientBootstrapOptionsMock)
        {
            var mock = new Mock<IOptions<ZeebeClientAcceleratorOptions>>();
            
            mock.SetupGet(m => m.Value).Returns(zeebeClientBootstrapOptionsMock.Object);

            return mock;
        }

        private static Mock<ZeebeClientAcceleratorOptions> CreateZeebeClientBootstrapOptionsMock(Mock<ClientOptions> zeebeClientOptionsMock, Mock<WorkerOptions> workerOptionsMock)
        {
            var mock = new Mock<ZeebeClientAcceleratorOptions>();

            mock.SetupGet(m => m.Client).Returns(zeebeClientOptionsMock.Object);
            mock.SetupGet(m => m.Worker).Returns(workerOptionsMock.Object);

            return mock;
        }

        private static Mock<WorkerOptions> CreateZeebeWorkerOptionsMock() 
        {            
            var mock = new Mock<WorkerOptions>();

            var random = new Random();

            mock.SetupGet(m => m.Name).Returns(Guid.NewGuid().ToString());
            mock.SetupGet(m => m.MaxJobsActive).Returns(random.Next(1, int.MaxValue));
            mock.SetupGet(m => m.HandlerThreads).Returns(Convert.ToByte(random.Next(1, 255)));
            mock.SetupGet(m => m.PollingTimeout).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.PollInterval).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.Timeout).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.RetryTimeout).Returns(TimeSpan.FromMilliseconds(random.Next()));
            mock.SetupGet(m => m.TenantIds).Returns(new string[] { Guid.NewGuid().ToString() });
            return mock;
        }

        private Mock<ClientOptions> CreateZeebeClientOptionsMock()
        {
            var mock = new Mock<ClientOptions>();

            var random = new Random();            

            return mock;
        }

        #endregion

    }
}