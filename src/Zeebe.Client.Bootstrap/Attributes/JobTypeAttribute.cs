using System;

namespace Zeebe.Client.Bootstrap.Attributes 
{    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JobTypeAttribute : Attribute
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