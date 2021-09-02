using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap
{
    public class BootstrapJobHandler : IBootstrapJobHandler
    {
        private readonly IJobHandlerProvider _jobHandlerProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IZeebeClient _client;
        private readonly IZeebeVariablesSerializer _serializer;
        private readonly ILogger<BootstrapJobHandler> _logger;

        public BootstrapJobHandler(IServiceProvider serviceProvider, IZeebeClient client, IJobHandlerProvider jobHandlerProvider, IZeebeVariablesSerializer serializer, ILogger<BootstrapJobHandler> logger)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._client = client ?? throw new ArgumentNullException(nameof(client));
            this._jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));   
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }

        public async Task HandleJob(IJob job, CancellationToken cancellationToken)
        {
            var jobHandlerInfo = this._jobHandlerProvider.JobHandlers
                .Where(i => job.Type.Equals(i.JobType))
                .FirstOrDefault();

            try 
            {
                await HandleJob(job, jobHandlerInfo, cancellationToken);
            }            
            catch(AbstractJobException ex) 
            {
                _logger.LogInformation(ex, $"JobException while handling job '${jobHandlerInfo.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                
                await this._client
                    .NewThrowErrorCommand(job.Key)
                    .ErrorCode(ex.Code)
                    .ErrorMessage(ex.Message)
                    .Send(cancellationToken);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, $"Unhandled exception while handling job '${jobHandlerInfo.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                throw;
            }
        }

        private async Task HandleJob(IJob job, IJobHandlerInfo jobHandlerInfo, CancellationToken cancellationToken)
        {
            if(jobHandlerInfo == null)
                throw new ArgumentNullException(nameof(jobHandlerInfo));

            var handlerInstance = _serviceProvider.GetService(jobHandlerInfo.Handler.DeclaringType);
            if(handlerInstance == null)
                throw new InvalidOperationException($"There is no service of type {jobHandlerInfo.Handler.DeclaringType}.");

            var jobType = jobHandlerInfo.Handler.GetParameters()[1].ParameterType;
            var abstractJob = CreateAbstractJobInstance(job, jobType);

            var response = jobHandlerInfo.Handler.Invoke(handlerInstance, new object[]  { _client, abstractJob, cancellationToken });

            _logger.LogInformation($"Job #{job.Key} ('{job.Type}') is handled by job handler '{jobHandlerInfo.Handler.DeclaringType.Name}'.");

            if (response is Task task) 
            {
                await task;
                response = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            var command = this._client.NewCompleteJobCommand(job.Key);
            
            if(response != null) {
                var variables = this._serializer.Serialize(response);
                command.Variables(variables);
            }
                
            await command.Send(cancellationToken);
        }

        private static object CreateAbstractJobInstance(IJob job, Type jobType) 
        {
            if(!jobType.IsSubclassOf(typeof(AbstractJob)))
                throw new Exception($"Type {jobType.FullName} is not a subclass of {typeof(AbstractJob).FullName}.");

            var constructor = jobType.GetConstructor(new Type[] { typeof(IJob) });
            if(constructor == null)
                throw new Exception($"Type {jobType.FullName} does not have a constructor with one parameter of type {typeof(IJob).FullName}.");

            return constructor.Invoke(new object[] { job });
        }
    }
}