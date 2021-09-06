using System;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    public class TimeoutAttribute : AbstractJobAttribute
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