using System;

using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes;

public class TenantIdsAttribute : AbstractWorkerAttribute
{
    public TenantIdsAttribute(params string[] tenantIds)
    {
        if (tenantIds is null || tenantIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(tenantIds));
        }

        TenantIds = tenantIds;
    }

    public string[] TenantIds { get; }
}