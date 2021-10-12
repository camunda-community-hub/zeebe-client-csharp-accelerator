using System;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public abstract class AbstractJob : IJob
    {
        private readonly IJob job;

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

    public abstract class AbstractJob<TState> : AbstractJob
        where TState : class, new()
    {
        private readonly TState state;

        public AbstractJob(IJob job, TState state)
            : base(job)
        {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public TState State { get { return state; } }
    }
}