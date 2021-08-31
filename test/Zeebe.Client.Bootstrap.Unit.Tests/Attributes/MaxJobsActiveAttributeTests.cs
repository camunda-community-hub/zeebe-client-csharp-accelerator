using System;
using Xunit;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Attributes 
{
    public class MaxJobsActiveAttributeTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ThrowsArgumentExceptionWhenMaxJobsActiveIsSmallerThenOne(int maxJobsActive)
        {
            Assert.Throws<ArgumentException>(nameof(maxJobsActive), () => new MaxJobsActiveAttribute(maxJobsActive));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var random = new Random();
            var expected = random.Next(1, int.MaxValue);
            var attribute = new MaxJobsActiveAttribute(expected);
            Assert.Equal(expected, attribute.MaxJobsActive);
        }
    }
}
