using System.Collections.Generic;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IJobHandlerInfoProvider 
    {
        IEnumerable<IJobHandlerInfo> JobHandlerInfoCollection { get; }
    }
}
