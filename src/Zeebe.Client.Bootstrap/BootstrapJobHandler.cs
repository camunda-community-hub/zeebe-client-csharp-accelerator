using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap
{
    public class BootstrapJobHandler : IBootstrapJobHandler
    {
        private readonly IJobHandlerInfoProvider jobHandlerInfoProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IZeebeVariablesSerializer serializer;
        private readonly IZeebeVariablesDeserializer deserializer;
        private readonly ILogger<BootstrapJobHandler> logger;

        public BootstrapJobHandler(IServiceProvider serviceProvider, IJobHandlerInfoProvider jobHandlerInfoProvider, IZeebeVariablesSerializer serializer, IZeebeVariablesDeserializer deserializer, ILogger<BootstrapJobHandler> logger)
        {            
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.jobHandlerInfoProvider = jobHandlerInfoProvider ?? throw new ArgumentNullException(nameof(jobHandlerInfoProvider));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken)
        {
            var jobHandlerInfo = this.jobHandlerInfoProvider.JobHandlerInfoCollection
                .Where(i => job.Type.Equals(i.JobType))
                .FirstOrDefault();

            try
            {
                var response = await HandleJob(job, jobHandlerInfo, cancellationToken);
                await CompleteJob(jobClient, job, response, cancellationToken);
            }
            catch (AbstractJobException ex)
            {
                await ThrowError(jobClient, job, jobHandlerInfo, ex, cancellationToken);
            }
            catch (Exception ex)
            {
                var jobException = ex.InnerException as AbstractJobException;
                if(jobException != null)
                {
                    await ThrowError(jobClient, job, jobHandlerInfo, jobException, cancellationToken);
                }
                else
                {
                    logger.LogError(ex, $"Unhandled exception while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                    throw;
                }
            }
        }

        private async Task<object> HandleJob(IJob job, IJobHandlerInfo jobHandlerInfo, CancellationToken cancellationToken)
        {
            if (jobHandlerInfo == null)
                throw new ArgumentNullException(nameof(jobHandlerInfo));

            var handlerInstance = serviceProvider.GetService(jobHandlerInfo.Handler.DeclaringType);
            if (handlerInstance == null)
                throw new InvalidOperationException($"There is no service of type {jobHandlerInfo.Handler.DeclaringType}.");

            var jobType = jobHandlerInfo.Handler.GetParameters()[0].ParameterType;
            var abstractJob = CreateAbstractJobInstance(job, jobType) ?? CreateGenericAbstractJobInstance(job, jobType);

            if(abstractJob == null)
                throw new Exception($"Type {jobType.FullName} could not be constructed.");

            var response = jobHandlerInfo.Handler.Invoke(handlerInstance, new object[] { abstractJob, cancellationToken });

            logger.LogInformation($"Job #{job.Key} ('{job.Type}') is handled by job handler '{jobHandlerInfo.Handler.DeclaringType.Name}'.");

            if (response is Task task)
            {
                await task;
                response = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            return response;
        }

        private async Task CompleteJob(IJobClient jobClient, IJob job, object response, CancellationToken cancellationToken)
        {
            var command = jobClient.NewCompleteJobCommand(job.Key);

            if (response != null)
            {
                var variables = this.serializer.Serialize(response);
                command.Variables(variables);
            }

            await command.Send(cancellationToken);
        }

        private async Task ThrowError(IJobClient jobClient, IJob job, IJobHandlerInfo jobHandlerInfo, AbstractJobException ex, CancellationToken cancellationToken)
        {
            logger.LogInformation(ex, $"JobException while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");

            await jobClient
                .NewThrowErrorCommand(job.Key)
                .ErrorCode(ex.Code)
                .ErrorMessage(ex.Message)
                .Send(cancellationToken);
        }

        private object CreateAbstractJobInstance(IJob job, Type jobType)
        {
            if (!jobType.IsSubclassOf(typeof(AbstractJob)))
                throw new Exception($"Type {jobType.FullName} is not a subclass of {typeof(AbstractJob).FullName}.");

            if (GetJobStateType(jobType) != null)
                return null;

            var constructor = jobType.GetConstructor(new Type[] { typeof(IJob) });
            if (constructor == null)
                throw new Exception($"Type {jobType.FullName} does not have a constructor with one parameter of type {typeof(IJob).FullName}.");

            return constructor.Invoke(new object[] { job });
        }

        private object CreateGenericAbstractJobInstance(IJob job, Type jobType)
        {
            var jobStateType = GetJobStateType(jobType);
            if(jobStateType == null)
                return null;

            var constructor = jobType.GetConstructor(new Type[] { typeof(IJob), jobStateType });
            if (constructor == null)
                throw new Exception($"Type {jobType.FullName} does not have a constructor with two parameters of type {typeof(IJob).FullName} and {jobStateType.FullName}.");

            var jobState = deserializer.Deserialize(job.Variables, jobStateType);
            return constructor.Invoke(new object[] { job, jobState });
        }

        private static Type GetJobStateType(Type jobType)
        {
            var definition = typeof(AbstractJob<>);

            var genericJobType = BaseTypes(jobType)
                .Where(t => t.IsAbstract 
                    && t.IsGenericType
                    && t.GetGenericTypeDefinition().Equals(definition))
                .SingleOrDefault();

            if(genericJobType == null)
                return null;

            return genericJobType.GetGenericArguments().Single();
        }
        private static IEnumerable<Type> BaseTypes(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}