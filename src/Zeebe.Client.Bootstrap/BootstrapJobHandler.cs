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
        private readonly IJobHandlerInfoProvider jobHandlerProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IZeebeClient client;
        private readonly IZeebeVariablesSerializer serializer;
        private readonly ILogger<BootstrapJobHandler> logger;

        public BootstrapJobHandler(IServiceProvider serviceProvider, IZeebeClient client, IJobHandlerInfoProvider jobHandlerProvider, IZeebeVariablesSerializer serializer, ILogger<BootstrapJobHandler> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));   
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }

        public async Task HandleJob(IJob job, CancellationToken cancellationToken)
        {
            var jobHandlerInfo = this.jobHandlerProvider.JobHandlerInfoCollection
                .Where(i => job.Type.Equals(i.JobType))
                .FirstOrDefault();

            try
            {
                var response = await HandleJob(job, jobHandlerInfo, cancellationToken);
                await CompleteJob(job, response, cancellationToken);
            }
            catch (AbstractJobException ex)
            {
                await ThrowError(job, jobHandlerInfo, ex, cancellationToken);
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, $"Unhandled exception while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                throw;
            }
        }

        private async Task<object> HandleJob(IJob job, IJobHandlerInfo jobHandlerInfo, CancellationToken cancellationToken)
        {
            if(jobHandlerInfo == null)
                throw new ArgumentNullException(nameof(jobHandlerInfo));

            var handlerInstance = serviceProvider.GetService(jobHandlerInfo.Handler.DeclaringType);
            if(handlerInstance == null)
                throw new InvalidOperationException($"There is no service of type {jobHandlerInfo.Handler.DeclaringType}.");

            var jobType = jobHandlerInfo.Handler.GetParameters()[0].ParameterType;
            var abstractJob = CreateAbstractJobInstance(job, jobType);

            var response = jobHandlerInfo.Handler.Invoke(handlerInstance, new object[]  { abstractJob, cancellationToken });

            logger.LogInformation($"Job #{job.Key} ('{job.Type}') is handled by job handler '{jobHandlerInfo.Handler.DeclaringType.Name}'.");

            if (response is Task task) 
            {
                await task;
                response = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            return response;
        }

        private async Task CompleteJob(IJob job, object response, CancellationToken cancellationToken)
        {
            var command = this.client.NewCompleteJobCommand(job.Key);

            if (response != null)
            {
                var variables = this.serializer.Serialize(response);
                command.Variables(variables);
            }

            await command.Send(cancellationToken);
        }

        private async Task ThrowError(IJob job, IJobHandlerInfo jobHandlerInfo, AbstractJobException ex, CancellationToken cancellationToken)
        {
            logger.LogInformation(ex, $"JobException while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");

            await this.client
                .NewThrowErrorCommand(job.Key)
                .ErrorCode(ex.Code)
                .ErrorMessage(ex.Message)
                .Send(cancellationToken);
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