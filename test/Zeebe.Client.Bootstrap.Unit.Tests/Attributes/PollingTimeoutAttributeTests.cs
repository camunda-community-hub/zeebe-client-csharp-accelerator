using System;
using Xunit;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Attributes 
{
    public class PollingTimeoutAttributeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentExceptionWhenPollingTimeoutIsSmallerThenOne(int pollingTimeoutInMilliseconds) {
            Assert.Throws<ArgumentException>(nameof(pollingTimeoutInMilliseconds), () => new PollingTimeoutAttribute(pollingTimeoutInMilliseconds));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var expected = (long)random.Next(1, int.MaxValue);
            var attribute = new PollingTimeoutAttribute(expected);
            Assert.Equal(expected, attribute.PollingTimeout.TotalMilliseconds);
        }
    }
}
