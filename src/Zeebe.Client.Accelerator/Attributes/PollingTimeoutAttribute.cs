using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class PollingTimeoutAttribute : AbstractWorkerAttribute
    {
        public PollingTimeoutAttribute(long pollingTimeoutInMilliseconds)
        {
            if (pollingTimeoutInMilliseconds < 1)
            {
                throw new ArgumentException($"'{nameof(pollingTimeoutInMilliseconds)}' cannot be smaller then one millisecond.", nameof(pollingTimeoutInMilliseconds));
            }

            this.PollingTimeout = TimeSpan.FromMilliseconds(pollingTimeoutInMilliseconds);
        }

        public TimeSpan PollingTimeout { get; }
    }
}