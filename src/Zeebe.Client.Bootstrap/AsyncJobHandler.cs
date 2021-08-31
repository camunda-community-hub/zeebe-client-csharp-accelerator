using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Options;

namespace Zeebe.Client.Bootstrap
{
    public class AsyncJobHandler : IAsyncJobHandler
    {
        private readonly IJobHandlerProvider jobHandlerProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IZeebeClient client;
        private readonly IZeebeVariablesSerializer serializer;
        private readonly ILogger<AsyncJobHandler> logger;

        public AsyncJobHandler(IJobHandlerProvider jobHandlerProvider, IServiceProvider serviceProvider, IZeebeClient client, IZeebeVariablesSerializer serializer, IOptions<ZeebeClientBootstrapOptions> options, ILogger<AsyncJobHandler> logger)
        {
            this.jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.client = client ?? throw new ArgumentNullException(nameof(client));   
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));   
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }


        public Task HandleJob(IJob job, CancellationToken cancellationToken)
        {
            var info = this.jobHandlerProvider.JobHandlers
                .Where(i => job.Type.Equals(i.JobType))
                .First();

            return HandleJob(job, info, cancellationToken);
        }

        private async Task HandleJob(IJob job, IJobHandlerInfo info, CancellationToken cancellationToken)
        {
            try 
            {
                var handlerInstance = serviceProvider.GetService(info.Handler.DeclaringType);
                if(handlerInstance == null)
                    throw new InvalidOperationException($"There is no service of type {info.Handler.DeclaringType}.");

                var jobType = info.Handler.GetParameters()[1].ParameterType;
                var abstractJob = CreateAbstractJobInstance(job, jobType);

                var response = info.Handler.Invoke(handlerInstance, new object[]  { client, abstractJob, cancellationToken });

                logger.LogInformation($"Job #{job.Key} ('{job.Type}') is handled by job handler '{info.Handler.DeclaringType.Name}'.");

                if (response is Task task) 
                {
                    await task;
                    response = task.GetType().GetProperty("Result")?.GetValue(task);
                }

                var command = this.client.NewCompleteJobCommand(job.Key);
                
                if(response != null) {
                    var variables = this.serializer.Serialize(response);
                    command.Variables(variables);
                }
                    
                await command.Send(cancellationToken);
            }            
            catch(JobException ex) 
            {
                logger.LogInformation(ex, $"JobException while handling job '${info.JobType}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                
                await this.client
                    .NewThrowErrorCommand(job.Key)
                    .ErrorCode(ex.Code)
                    .ErrorMessage(ex.Message)
                    .Send(cancellationToken);
            }
            catch(Exception ex) 
            {
                logger.LogError(ex, $"Unhandled exception while handling job '${info.JobType}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                throw;
            }
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