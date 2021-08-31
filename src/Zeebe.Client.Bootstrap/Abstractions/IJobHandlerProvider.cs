using System.Collections.Generic;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IJobHandlerProvider 
    {
        IEnumerable<IJobHandlerInfo> JobHandlers { get; }
    }
}
