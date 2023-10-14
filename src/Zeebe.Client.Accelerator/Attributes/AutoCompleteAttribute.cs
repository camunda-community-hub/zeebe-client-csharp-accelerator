using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class AutoCompleteAttribute : AbstractWorkerAttribute
    {

        public AutoCompleteAttribute(bool autoComplete)
        {
            this.AutoComplete = autoComplete;
        }

        public bool AutoComplete { get; }
    }
}