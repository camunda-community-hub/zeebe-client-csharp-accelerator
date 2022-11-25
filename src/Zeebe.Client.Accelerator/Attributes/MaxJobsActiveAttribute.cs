using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class MaxJobsActiveAttribute : AbstractJobAttribute
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