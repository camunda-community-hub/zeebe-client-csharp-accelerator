using System;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TimeoutAttribute : Attribute
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