using System;
using Xunit;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Attributes 
{
    public class WorkerNameAttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenJobTypeIsNullOrEmptyOrWhiteSpace(string workerName) {
            Assert.Throws<ArgumentException>("workerName", () => new WorkerNameAttribute(workerName));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var workerName = Guid.NewGuid().ToString();
            var attribute = new WorkerNameAttribute(workerName);
            Assert.NotNull(attribute.WorkerName);
            Assert.Equal(workerName, attribute.WorkerName);
        }
    }
}
