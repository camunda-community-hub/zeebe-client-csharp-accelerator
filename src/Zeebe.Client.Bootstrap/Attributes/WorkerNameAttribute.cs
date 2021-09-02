using System;

namespace Zeebe.Client.Bootstrap.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class WorkerNameAttribute : Attribute
    {
        public WorkerNameAttribute(string workerName)
        {
            if (string.IsNullOrWhiteSpace(workerName))
            {
                throw new ArgumentException($"'{nameof(workerName)}' cannot be null or whitespace.", nameof(workerName));
            }

            this.WorkerName = workerName;
        }

        public string WorkerName { get; }
    }
}