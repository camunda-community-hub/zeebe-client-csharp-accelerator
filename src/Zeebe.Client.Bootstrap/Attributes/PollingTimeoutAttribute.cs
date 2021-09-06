using System;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    public class PollingTimeoutAttribute : AbstractJobAttribute
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