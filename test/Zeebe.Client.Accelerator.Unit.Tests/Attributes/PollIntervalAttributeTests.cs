using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{
    public class PollIntervalAttributeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentExceptionWhenPollIntervalIsSmallerThenOne(long pollIntervalInMilliseconds)
        {
            Assert.Throws<ArgumentException>(nameof(pollIntervalInMilliseconds), () => new PollIntervalAttribute(pollIntervalInMilliseconds));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var expected = (long)random.Next(1, int.MaxValue);
            var attribute = new PollIntervalAttribute(expected);
            Assert.Equal(expected, attribute.PollInterval.TotalMilliseconds);
        }
    }
}
