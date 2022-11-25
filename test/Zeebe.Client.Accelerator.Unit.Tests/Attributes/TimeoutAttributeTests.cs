using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{
    public class TimeoutAttributeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentExceptionWhenTimeoutIsSmallerThenOne(long timeoutInMilliseconds)
        {
            Assert.Throws<ArgumentException>(nameof(timeoutInMilliseconds), () => new TimeoutAttribute(timeoutInMilliseconds));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var expected = (long)random.Next(1, int.MaxValue);
            var attribute = new TimeoutAttribute(expected);
            Assert.Equal(expected, attribute.Timeout.TotalMilliseconds);
        }
    }
}
