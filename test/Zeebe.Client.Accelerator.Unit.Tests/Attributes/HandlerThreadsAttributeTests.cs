using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{
    public class HandlerThreadsAttributeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentExceptionWhenHandlerThreadsIsSmallerThenOne(int maxJobsActive)
        {
            Assert.Throws<ArgumentException>(nameof(maxJobsActive), () => new MaxJobsActiveAttribute(maxJobsActive));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var expected = Convert.ToByte(random.Next(1, 255));
            var attribute = new HandlerThreadsAttribute(expected);
            Assert.Equal(expected, attribute.HandlerThreads);
        }
    }
}
