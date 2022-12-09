using System;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Accelerator.Abstractions
{
     /// <summary>
     /// Data of a Job. Not intended for direct usage - please use class ZeebeJob.
     /// </summary>
    public abstract class AbstractJob : IJob
    {
        protected readonly IJob job;

        public AbstractJob(IJob job)
        {
            if (job is null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            this.job = job;
        }

        public long Key => job.Key;

        public string Type => job.Type;

        public long ProcessInstanceKey => job.ProcessInstanceKey;

        public string BpmnProcessId => job.BpmnProcessId;

        public int ProcessDefinitionVersion => job.ProcessDefinitionVersion;

        public long ProcessDefinitionKey => job.ProcessDefinitionKey;

        public string ElementId => job.ElementId;

        public long ElementInstanceKey => job.ElementInstanceKey;

        public string Worker => job.Worker;

        public int Retries => job.Retries;

        public DateTime Deadline => job.Deadline;

        public string Variables => job.Variables;

        public string CustomHeaders => job.CustomHeaders;
    }

}