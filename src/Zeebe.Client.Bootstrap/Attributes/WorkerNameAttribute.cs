using System;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Attributes
{
    public class WorkerNameAttribute : AbstractJobHandlerAttribute
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