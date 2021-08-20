using System;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PollIntervalAttribute : Attribute
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