using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Bootstrap.Options;
using Microsoft.Extensions.Options;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap
{    public class ZeebeHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IZeebeClient client;
        private readonly IJobHandlerProvider jobHandlerProvider;
        private readonly WorkerOptions zeebeWorkerOptions;
        private readonly ILogger<ZeebeHostedService> logger;
        private List<IJobWorker> workers = new List<IJobWorker>();

        public ZeebeHostedService(IServiceProvider serviceProvider, IZeebeClient client, IJobHandlerProvider jobHandlerProvider, IOptions<ZeebeClientBootstrapOptions> options, ILogger<ZeebeHostedService> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.client = client ?? throw new ArgumentNullException(nameof(client));   
            this.jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this.zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientBootstrapOptions>)}.Value.{nameof(ZeebeClientBootstrapOptions.Worker)} is null.");
            ValidateZeebeWorkerOptions(zeebeWorkerOptions);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach(var reference in jobHandlerProvider.JobHandlers) 
            {
                var worker = client.NewWorker()
                    .JobType(reference.JobType)                    
                    .Handler((client, job) =>  HandleJob(client, job, cancellationToken, reference))                
                    .FetchVariables(reference.FetchVariabeles)
                    .MaxJobsActive(reference.MaxJobsActive.HasValue ? reference.MaxJobsActive.Value : zeebeWorkerOptions.MaxJobsActive)
                    .Name(zeebeWorkerOptions.Name ?? reference.WorkerName)
                    .PollingTimeout(reference.PollingTimeout.HasValue ? reference.PollingTimeout.Value : zeebeWorkerOptions.PollingTimeout)
                    .PollInterval(reference.PollInterval.HasValue ? reference.PollInterval.Value : zeebeWorkerOptions.PollInterval)
                    .Timeout(reference.Timeout.HasValue ? reference.Timeout.Value : zeebeWorkerOptions.Timeout)
                    .Open();

                logger.LogInformation($"Created Zeebe job worker to delegate Zeebe job '{reference.JobType}' handling to JobHandler '{reference.Handler.DeclaringType}'.");

                workers.Add(worker);
            }

            logger.LogInformation($"Created {workers.Count} Zeebe job workers.");

            return Task.CompletedTask;
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            workers.ForEach(w => w.Dispose());
            workers.Clear();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            workers.ForEach(w => w.Dispose());
            workers.Clear();
        }

        private static void ValidateZeebeWorkerOptions(WorkerOptions zeebeWorkerOptions)
        {
            if (zeebeWorkerOptions.MaxJobsActive < 1) 
                throw new ArgumentOutOfRangeException($"{nameof(WorkerOptions)}.{nameof(zeebeWorkerOptions.MaxJobsActive)}");
            if(zeebeWorkerOptions.Timeout.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException($"{nameof(WorkerOptions)}.{nameof(zeebeWorkerOptions.Timeout)}");
            if(zeebeWorkerOptions.PollInterval.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException($"{nameof(WorkerOptions)}.{nameof(zeebeWorkerOptions.PollInterval)}");
            if(zeebeWorkerOptions.PollingTimeout.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException($"{nameof(WorkerOptions)}.{nameof(zeebeWorkerOptions.PollingTimeout)}");
            if(String.IsNullOrWhiteSpace(zeebeWorkerOptions.Name) && zeebeWorkerOptions.Name != null)
                throw new ArgumentException($"'{nameof(zeebeWorkerOptions.Name)}' cannot be empty or whitespace.", $"{nameof(WorkerOptions)}.{nameof(zeebeWorkerOptions.Name)}");
        }

        private Task HandleJob(IJobClient client, IJob job, CancellationToken cancellationToken, IJobHandlerReference reference) {
            try 
            {
                var handlerInstance = serviceProvider.GetService(reference.Handler.DeclaringType);
                if(handlerInstance == null)
                    throw new InvalidOperationException($"There is no service of type {reference.Handler.DeclaringType}.");

                var jobType = reference.Handler.GetParameters()[1].ParameterType;
                var abstractJob = CreateAbstractJobInstance(job, jobType);

                var response = reference.Handler.Invoke(handlerInstance, new object[]  { client, abstractJob, cancellationToken });

                var task = response as Task;
                if(task != null)
                    return task;

                
                return Task.CompletedTask;
            }
            catch(Exception ex) 
            {
                logger.LogError(ex, $"Exception while handling job '${reference.JobType}' with key '${job.Key}'. Process instance key = $'{job.ProcessInstanceKey}', process definition key = '{job.ProcessDefinitionKey}', process definition version = '{job.ProcessDefinitionVersion}'.");
                return Task.FromException(ex);
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