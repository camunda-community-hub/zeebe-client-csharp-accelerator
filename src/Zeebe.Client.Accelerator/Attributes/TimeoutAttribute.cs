using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class TimeoutAttribute : AbstractJobHandlerAttribute
    {
        public TimeoutAttribute(long timeoutInMilliseconds)
        {
            if (timeoutInMilliseconds < 1)
            {
                throw new ArgumentException($"'{nameof(timeoutInMilliseconds)}' cannot be smaller then one millisecond.", nameof(timeoutInMilliseconds));
            }

            this.Timeout = TimeSpan.FromMilliseconds(timeoutInMilliseconds);
        }

        public TimeSpan Timeout { get; }
    }
}