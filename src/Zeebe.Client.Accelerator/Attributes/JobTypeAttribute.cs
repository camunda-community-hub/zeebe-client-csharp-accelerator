using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{
    public class JobTypeAttribute : AbstractJobAttribute
    {
        public JobTypeAttribute(string jobType)
        {
            if (string.IsNullOrWhiteSpace(jobType))
            {
                throw new ArgumentException($"'{nameof(jobType)}' cannot be null or whitespace.", nameof(jobType));
            }

            this.JobType = jobType;
        }

        public string JobType { get; }
    }
}