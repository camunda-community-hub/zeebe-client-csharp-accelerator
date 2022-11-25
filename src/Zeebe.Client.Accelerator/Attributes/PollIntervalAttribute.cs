using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class PollIntervalAttribute : AbstractJobAttribute
    {
        public PollIntervalAttribute(long pollIntervalInMilliseconds)
        {
            if (pollIntervalInMilliseconds < 1)
            {
                throw new ArgumentException($"'{nameof(pollIntervalInMilliseconds)}' cannot be smaller then one millisecond.", nameof(pollIntervalInMilliseconds));
            }

            this.PollInterval = TimeSpan.FromMilliseconds(pollIntervalInMilliseconds);
        }

        public TimeSpan PollInterval { get; }
    }
}