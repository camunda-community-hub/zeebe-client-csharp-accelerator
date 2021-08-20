using System;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PollingTimeoutAttribute : Attribute
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