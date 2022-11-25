using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{
    public class JobTypeAttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenJobTypeIsNullOrEmptyOrWhiteSpace(string jobType)
        {
            Assert.Throws<ArgumentException>(nameof(jobType), () => new JobTypeAttribute(jobType));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var jobType = Guid.NewGuid().ToString();
            var attribute = new JobTypeAttribute(jobType);
            Assert.NotNull(attribute.JobType);
            Assert.Equal(jobType, attribute.JobType);
        }
    }
}
