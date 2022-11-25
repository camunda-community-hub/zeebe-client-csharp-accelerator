using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{
    public class WorkerNameAttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenJobTypeIsNullOrEmptyOrWhiteSpace(string workerName)
        {
            Assert.Throws<ArgumentException>(nameof(workerName), () => new WorkerNameAttribute(workerName));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var exptected = Guid.NewGuid().ToString();
            var attribute = new WorkerNameAttribute(exptected);
            Assert.NotNull(attribute.WorkerName);
            Assert.Equal(exptected, attribute.WorkerName);
        }
    }
}
