using System;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    public class FetchVariablesAttribute : AbstractJobAttribute
    {
        public FetchVariablesAttribute(params string[] fetchVariables)
        {
            if (fetchVariables is null || fetchVariables.Length == 0)
            {
                throw new ArgumentNullException(nameof(fetchVariables));
            }

            this.FetchVariables = fetchVariables;
        }

        public string[] FetchVariables { get; }
    }
}