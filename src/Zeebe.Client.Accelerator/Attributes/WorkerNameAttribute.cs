using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes
{
    public class WorkerNameAttribute : AbstractWorkerAttribute
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