using System.Collections.Generic;
using System.Reflection;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IAssemblyProvider 
    {
        IEnumerable<Assembly> Assemblies { get; }
    }
}