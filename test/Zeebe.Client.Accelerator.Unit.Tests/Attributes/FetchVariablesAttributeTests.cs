using System;
using Xunit;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes 
{    
    public class FetchVariablesAttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new string[] { } })]
        public void ThrowsArgumentExceptionWhenFetchVariablesIsNullOrEmptyOrWhiteSpace(string[] fetchVariables)
        {
            Assert.Throws<ArgumentNullException>(nameof(fetchVariables), () => new FetchVariablesAttribute(fetchVariables));
        }

        [Fact]
        public void AllPropertiesAreSetWhenCreated()
        {   
            var expected = new string[] 
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            }; 
            var attribute = new FetchVariablesAttribute(expected);
            Assert.NotNull(attribute.FetchVariables);
            Assert.Equal(expected, attribute.FetchVariables);
        }
    }
}
