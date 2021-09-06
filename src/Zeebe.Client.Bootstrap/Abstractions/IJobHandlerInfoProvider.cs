using System.Collections.Generic;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IJobHandlerInfoProvider 
    {
        IEnumerable<IJobHandlerInfo> JobHandlerInfoCollection { get; }
    }
}
