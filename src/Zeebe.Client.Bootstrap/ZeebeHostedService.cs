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
        private readonly IBootstrapJobHandler _bootstrapJobHandler;
        private readonly IZeebeClient _client;
        private readonly IJobHandlerProvider _jobHandlerProvider;
        private readonly WorkerOptions _zeebeWorkerOptions;
        private readonly ILogger<ZeebeHostedService> _logger;
        private readonly List<IJobWorker> _workers = new List<IJobWorker>();

        public ZeebeHostedService(IBootstrapJobHandler bootstrapJobHandler, IZeebeClient client, IJobHandlerProvider jobHandlerProvider, IOptions<ZeebeClientBootstrapOptions> options, ILogger<ZeebeHostedService> logger)
        {
            this._bootstrapJobHandler = bootstrapJobHandler ?? throw new ArgumentNullException(nameof(bootstrapJobHandler));
            this._client = client ?? throw new ArgumentNullException(nameof(client));   
            this._jobHandlerProvider = jobHandlerProvider ?? throw new ArgumentNullException(nameof(jobHandlerProvider));
            this._zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientBootstrapOptions>)}.Value.{nameof(ZeebeClientBootstrapOptions.Worker)} is null.");
            ValidateZeebeWorkerOptions(_zeebeWorkerOptions);
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));;            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach(var info in _jobHandlerProvider.JobHandlers) 
            {
                var worker = _client.NewWorker()
                    .JobType(info.JobType)                    
                    .Handler((client, job) =>  this._bootstrapJobHandler.HandleJob(job, cancellationToken))                
                    .FetchVariables(info.FetchVariabeles)
                    .MaxJobsActive(info.MaxJobsActive ?? _zeebeWorkerOptions.MaxJobsActive)
                    .Name(_zeebeWorkerOptions.Name ?? info.WorkerName)
                    .PollingTimeout(info.PollingTimeout ?? _zeebeWorkerOptions.PollingTimeout)
                    .PollInterval(info.PollInterval ?? _zeebeWorkerOptions.PollInterval)
                    .Timeout(info.Timeout ?? _zeebeWorkerOptions.Timeout)
                    .Open();

                _logger.LogInformation($"Created job worker to delegate job '{info.JobType}' to the boostrap job handler.");

                _workers.Add(worker);
            }

            _logger.LogInformation($"Created {_workers.Count} job workers.");

            return Task.CompletedTask;            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _workers.ForEach(w => w.Dispose());
            _workers.Clear();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _workers.ForEach(w => w.Dispose());
            _workers.Clear();
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