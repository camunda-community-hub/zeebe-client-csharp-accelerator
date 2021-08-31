using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Api.Worker;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Bootstrap.Options;
using Microsoft.Extensions.Options;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap
{
    public class ZeebeHostedService : IHostedService, IDisposable
    {
        private readonly IAsyncJobHandler jobHandler;
        private readonly IZeebeClient client;
        private readonly IJobHandlerProvider jobHandlerProvider;
        private readonly WorkerOptions zeebeWorkerOptions;
        private readonly ILogger<ZeebeHostedService> logger;
        private readonly List<IJobWorker> workers = new List<IJobWorker>();

        public ZeebeHostedService(IAsyncJobHandler jobHandler, IZeebeClient client, IJobHandlerProvider jobHandlerProvider, IOptions<ZeebeClientBootstrapOptions> options, ILogger<ZeebeHostedService> logger)
        {
            this.jobHandler = jobHandler ?? throw new ArgumentNullException(nameof(jobHandler));
            this.client = client ?? throw new ArgumentNullException(nameof(client));   
            this.jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this.zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientBootstrapOptions>)}.Value.{nameof(ZeebeClientBootstrapOptions.Worker)} is null.");
            ValidateZeebeWorkerOptions(zeebeWorkerOptions);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach(var info in jobHandlerProvider.JobHandlers) 
            {
                var worker = client.NewWorker()
                    .JobType(info.JobType)                    
                    .Handler((client, job) =>  this.jobHandler.HandleJob(job, cancellationToken))                
                    .FetchVariables(info.FetchVariabeles)
                    .MaxJobsActive(info.MaxJobsActive ?? zeebeWorkerOptions.MaxJobsActive)
                    .Name(zeebeWorkerOptions.Name ?? info.WorkerName)
                    .PollingTimeout(info.PollingTimeout ?? zeebeWorkerOptions.PollingTimeout)
                    .PollInterval(info.PollInterval ?? zeebeWorkerOptions.PollInterval)
                    .Timeout(info.Timeout ?? zeebeWorkerOptions.Timeout)
                    .Open();

                logger.LogInformation($"Created job worker to delegate job '{info.JobType}' handling to handler '{info.Handler.DeclaringType}'.");

                workers.Add(worker);
            }

            logger.LogInformation($"Created {workers.Count} job workers.");

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
    }
}