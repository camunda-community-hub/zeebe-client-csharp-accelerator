using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.ConnectorSecrets;
using Zeebe.Client.Accelerator.Options;

namespace Zeebe.Client.Accelerator
{
    public class ZeebeJobHandler : IBootstrapJobHandler
    {
        private readonly IJobHandlerInfoProvider jobHandlerInfoProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IZeebeVariablesSerializer serializer;
        private readonly IZeebeVariablesDeserializer deserializer;
        private readonly ZeebeClientAcceleratorOptions.WorkerOptions zeebeWorkerOptions;
        private readonly ILogger<ZeebeJobHandler> logger;
        private readonly ISecretHandler secretHandler;

        public ZeebeJobHandler(IServiceProvider serviceProvider, IJobHandlerInfoProvider jobHandlerInfoProvider, IZeebeVariablesSerializer serializer, IZeebeVariablesDeserializer deserializer, IOptions<ZeebeClientAcceleratorOptions> options, ILogger<ZeebeJobHandler> logger, ISecretHandler secretHandler)
        {            
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.jobHandlerInfoProvider = jobHandlerInfoProvider ?? throw new ArgumentNullException(nameof(jobHandlerInfoProvider));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            this.zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientAcceleratorOptions>)}.Value.{nameof(ZeebeClientAcceleratorOptions.Worker)} is null.");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.secretHandler = secretHandler ?? throw new ArgumentNullException(nameof(secretHandler));
        }

        public async Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken)
        {
            var jobHandlerInfo = this.jobHandlerInfoProvider.JobHandlerInfoCollection
                .Where(i => job.Type.Equals(i.JobType))
                .FirstOrDefault();

            try
            {
                var response = await HandleJob(jobClient, job, jobHandlerInfo, cancellationToken);
                if (jobHandlerInfo.AutoComplete)
                {
                    await CompleteJob(jobClient, job, response, cancellationToken);
                } 
            }
            catch (BpmnErrorException ex)
            {
                await ThrowError(jobClient, job, jobHandlerInfo, ex, cancellationToken);
            }
            catch (Exception ex)
            {
                var jobException = ex.InnerException as BpmnErrorException;
                if (jobException != null)
                {
                    await ThrowError(jobClient, job, jobHandlerInfo, jobException, cancellationToken);
                }
                else
                {
                    var innerEx = ex.InnerException;
                    await ThrowException(jobClient, job, jobHandlerInfo, innerEx != null ? innerEx : ex, cancellationToken);
                }
            }
        }

        private async Task<object> HandleJob(IJobClient jobClient, IJob job, IJobHandlerInfo jobHandlerInfo, CancellationToken cancellationToken)
        {
            if (jobHandlerInfo == null)
                throw new ArgumentNullException(nameof(jobHandlerInfo));

            var handlerInstance = serviceProvider.GetService(jobHandlerInfo.Handler.ReflectedType);
            if (handlerInstance == null)
                throw new InvalidOperationException($"There is no service of type {jobHandlerInfo.Handler.ReflectedType}.");

            var jobType = jobHandlerInfo.Handler.GetParameters()[0].ParameterType;
            var abstractJob = CreateAbstractJobInstance(jobClient, job, jobType) ?? CreateGenericAbstractJobInstance(job, jobType);

            if (abstractJob == null)
                throw new Exception($"Type {jobType.FullName} could not be constructed.");

            var response = jobHandlerInfo.Handler.Invoke(handlerInstance, new object[] { abstractJob, cancellationToken });

            logger.LogDebug($"Job #{job.Key} ('{job.Type}') is handled by job handler '{jobHandlerInfo.Handler.ReflectedType.Name}'.");

            if (response is Task task)
            {
                await task;
                response = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            return response;
        }

        private async Task CompleteJob(IJobClient jobClient, IJob job, object response, CancellationToken cancellationToken)
        {
            var completeJobCommand = jobClient.NewCompleteJobCommand(job.Key);

            if (response != null)
            {
                var variables = this.serializer.Serialize(response);
                completeJobCommand.Variables(variables);
            }

            await completeJobCommand.SendWithRetry(this.zeebeWorkerOptions.RetryTimeout, cancellationToken);
        }

        private async Task ThrowError(IJobClient jobClient, IJob job, IJobHandlerInfo jobHandlerInfo, BpmnErrorException ex, CancellationToken cancellationToken)
        {
            logger.LogDebug(ex, $"BpmnErrorException while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");

            await jobClient
                .NewThrowErrorCommand(job.Key)
                .ErrorCode(ex.Code)
                .ErrorMessage(ex.Message)
                .Send(this.zeebeWorkerOptions.RetryTimeout, cancellationToken);
        }

        private async Task ThrowException(IJobClient jobClient, IJob job, IJobHandlerInfo jobHandlerInfo, Exception ex, CancellationToken cancellationToken)
        {
            logger.LogError(ex, $"Unhandled exception while handling job '${jobHandlerInfo?.JobType ?? "null"}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");

            await jobClient
                .NewFailCommand(job.Key)
                .Retries(job.Retries - 1)
                .ErrorMessage(ex.GetType().Name + ": " + ex.Message)
                .Send(this.zeebeWorkerOptions.RetryTimeout, cancellationToken);
        }

        private object CreateAbstractJobInstance(IJobClient jobClient, IJob job, Type jobType)
        {
            if (!jobType.IsSubclassOf(typeof(AbstractJob)))
                throw new Exception($"Type {jobType.FullName} is not a subclass of {typeof(AbstractJob).FullName}.");

            if (GetJobStateType(jobType) != null)
                return null;

            if (typeof(ZeebeJob).IsAssignableFrom(jobType))
            {
                var constructor = jobType.GetConstructor(new Type[] { typeof(IJobClient), typeof(IJob), typeof(IZeebeVariablesDeserializer), typeof(ISecretHandler) });
                if (constructor == null)
                    throw new Exception($"Type {jobType.FullName} does not have a constructor with two parameters of type {typeof(IJob).FullName},{typeof(IZeebeVariablesDeserializer).FullName}.");

                return constructor.Invoke(new object[] { jobClient, job, deserializer, secretHandler });
            }
            else
            {
                var constructor = jobType.GetConstructor(new Type[] { typeof(IJob) });
                if (constructor == null)
                    throw new Exception($"Type {jobType.FullName} does not have a constructor with one parameter of type {typeof(IJob).FullName}.");

                return constructor.Invoke(new object[] { job });
            }
        }

        private object CreateGenericAbstractJobInstance(IJob job, Type jobType)
        {
            var jobStateType = GetJobStateType(jobType);
            if (jobStateType == null)
                return null;

            var constructor = jobType.GetConstructor(new Type[] { typeof(IJob), jobStateType });
            if (constructor == null)
                throw new Exception($"Type {jobType.FullName} does not have a constructor with two parameters of type {typeof(IJob).FullName} and {jobStateType.FullName}.");

            var jobState = deserializer.Deserialize(job.Variables, jobStateType);
            return constructor.Invoke(new object[] { job, jobState });
        }

        private static Type GetJobStateType(Type jobType)
        {
            var definition = typeof(AbstractJob);

            var genericJobType = BaseTypes(jobType)
                .Where(t => t.IsAbstract
                    && t.IsGenericType
                    && t.GetGenericTypeDefinition().Equals(definition))
                .SingleOrDefault();

            if (genericJobType == null)
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