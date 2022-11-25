using System;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public class ZeebeJob : AbstractJob
    {
        protected readonly IZeebeVariablesDeserializer _variablesDeserializer;

        public ZeebeJob(IJob job, IZeebeVariablesDeserializer variablesDeserializer) : base(job)
        {
            _variablesDeserializer = variablesDeserializer;
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

        public ZeebeJob(IJob job, IZeebeVariablesDeserializer variablesDeserializer)
            : base(job, variablesDeserializer) { }

        public TState getVariables()
        {
            return _variablesDeserializer.Deserialize<TState>(job.Variables);
        }
    }
}
