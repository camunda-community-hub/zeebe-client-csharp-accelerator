using System;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MaxJobsActiveAttribute : Attribute
    {
        public MaxJobsActiveAttribute(int maxJobsActive)
        {
            if (maxJobsActive < 1)
            {
                throw new ArgumentException($"'{nameof(maxJobsActive)}' cannot be smaller then 1.", nameof(maxJobsActive));
            }

            this.MaxJobsActive = maxJobsActive;
        }

        public int MaxJobsActive { get; }
    }
}