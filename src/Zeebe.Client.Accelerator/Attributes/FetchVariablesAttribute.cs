using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class FetchVariablesAttribute : AbstractWorkerAttribute
    {
        public FetchVariablesAttribute(params string[] fetchVariables)
        {
            if (fetchVariables is null || fetchVariables.Length == 0)
            {
                throw new ArgumentNullException(nameof(fetchVariables));
            }

            this.None = false;
            this.FetchVariables = fetchVariables;
        }

        public string[] FetchVariables { get; }

        public FetchVariablesAttribute(bool none)
        {
            this.None = none;
            this.FetchVariables = new string[0];
        }

        public bool None { get; }
    }
}