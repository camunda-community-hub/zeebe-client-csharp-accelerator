using Xunit;
using System;
using Zeebe.Client.Bootstrap.Unit.Tests.Stubs;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Abstractions
{
    public class AbstractJobExceptionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenCodeIsNullOrEmptyOrWhiteSpace(string code)
        {
            Assert.Throws<ArgumentException>("code", () => new SimpleJobException(code, Guid.NewGuid().ToString()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ThrowsArgumentExceptionWhenMessageIsNullOrEmptyOrWhiteSpace(string message)
        {
            Assert.Throws<ArgumentException>("message", () => new SimpleJobException(Guid.NewGuid().ToString(), message));
        }


        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var code = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToString();

            var exception = new SimpleJobException(code, message);

            Assert.Equal(code, exception.Code);
            Assert.Equal(message, exception.Message);
        }
    }
}