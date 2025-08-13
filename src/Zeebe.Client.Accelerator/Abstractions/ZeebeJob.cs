using System;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public class ZeebeJob : AbstractJob
    {
        protected readonly IZeebeVariablesDeserializer _variablesDeserializer;
        protected readonly IJobClient _jobClient;
        protected readonly ISecretHandler _secretHandler;

        public ZeebeJob(IJobClient jobClient, IJob job, IZeebeVariablesDeserializer variablesDeserializer, ISecretHandler secretHandler) : base(job)
        {
            _jobClient = jobClient;
            _variablesDeserializer = variablesDeserializer;
            _secretHandler = secretHandler;
        }

        public IJobClient GetClient() 
        { 
            return _jobClient; 
        }
        public T getVariables<T>()
        {
            var processedVariables = _secretHandler.ReplaceSecretsAsync(job.Variables).GetAwaiter().GetResult();
            return _variablesDeserializer.Deserialize<T>(processedVariables);
        }
        public async Task<T> getVariablesAsync<T>()
        {
            var processedVariables = await _secretHandler.ReplaceSecretsAsync(job.Variables);
            return _variablesDeserializer.Deserialize<T>(processedVariables);
        }
        public T getCustomHeaders<T>()
        {
            return _variablesDeserializer.Deserialize<T>(job.CustomHeaders);
        }
    }

    public class ZeebeJob<TState> : ZeebeJob
        where TState : class, new()
    {

        public ZeebeJob(IJobClient jobClient, IJob job, IZeebeVariablesDeserializer variablesDeserializer, ISecretHandler secretHandler)
            : base(jobClient, job, variablesDeserializer, secretHandler) { }

        public TState getVariables()
        {
            var processedVariables = _secretHandler.ReplaceSecretsAsync(job.Variables).GetAwaiter().GetResult();
            return _variablesDeserializer.Deserialize<TState>(processedVariables);
        }
        
        public async Task<TState> getVariablesAsync()
        {
            var processedVariables = await _secretHandler.ReplaceSecretsAsync(job.Variables);
            return _variablesDeserializer.Deserialize<TState>(processedVariables);
        }
    }
}
