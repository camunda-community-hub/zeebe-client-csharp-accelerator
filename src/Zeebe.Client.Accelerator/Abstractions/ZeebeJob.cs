using System;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public class ZeebeJob : AbstractJob
    {
        protected readonly IZeebeVariablesDeserializer _variablesDeserializer;
        protected readonly IJobClient _jobClient;

        public ZeebeJob(IJobClient jobClient, IJob job, IZeebeVariablesDeserializer variablesDeserializer) : base(job)
        {
            _jobClient = jobClient;
            _variablesDeserializer = variablesDeserializer;
        }

        public IJobClient GetClient() 
        { 
            return _jobClient; 
        }
        public T getVariables<T>()
        {
            return _variablesDeserializer.Deserialize<T>(job.Variables);
        }
        public T getCustomHeaders<T>()
        {
            return _variablesDeserializer.Deserialize<T>(job.CustomHeaders);
        }
    }

    public class ZeebeJob<TState> : ZeebeJob
        where TState : class, new()
    {

        public ZeebeJob(IJobClient jobClient, IJob job, IZeebeVariablesDeserializer variablesDeserializer)
            : base(jobClient, job, variablesDeserializer) { }

        public TState getVariables()
        {
            return _variablesDeserializer.Deserialize<TState>(job.Variables);
        }
    }
}
