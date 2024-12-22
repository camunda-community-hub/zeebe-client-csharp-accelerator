using System;

using Xunit;

using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Attributes;
public class TenantIdsAttributeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(new object[] { new string[] { } })]
    public void ThrowsArgumentExceptionWhenFetchVariablesIsNullOrEmptyOrWhiteSpace(string[] tenantIds)
    {
        Assert.Throws<ArgumentNullException>(nameof(tenantIds), () => new TenantIdsAttribute(tenantIds));
    }

    [Fact]
    public void AllPropertiesAreSetWhenCreated()
    {
        string[] expectedTenantIds = new string[]
        {
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        };

        var attribute = new TenantIdsAttribute(expectedTenantIds);
        Assert.NotNull(attribute.TenantIds);
        Assert.Equal(expectedTenantIds, attribute.TenantIds);
    }
}


