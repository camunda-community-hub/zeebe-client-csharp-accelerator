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
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Bootstrap
{
    public class ZeebeHostedService : IHostedService, IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly IServiceScope serviceScope;
        private readonly IZeebeClient zeebeClient;
        private readonly IJobHandlerInfoProvider jobHandlerInfoProvider;
        private readonly WorkerOptions zeebeWorkerOptions;
        private readonly ILogger<ZeebeHostedService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly List<IJobWorker> workers = new List<IJobWorker>();

        public ZeebeHostedService(IServiceScopeFactory serviceScopeFactory, IJobHandlerInfoProvider jobHandlerInfoProvider, IOptions<ZeebeClientBootstrapOptions> options, ILogger<ZeebeHostedService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.jobHandlerInfoProvider = jobHandlerInfoProvider ?? throw new ArgumentNullException(nameof(jobHandlerInfoProvider));
            this.zeebeWorkerOptions = options?.Value?.Worker ?? throw new ArgumentNullException(nameof(options), $"{nameof(IOptions<ZeebeClientBootstrapOptions>)}.Value.{nameof(ZeebeClientBootstrapOptions.Worker)} is null.");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceScope = serviceScopeFactory.CreateScope();
            this.zeebeClient = serviceScope.ServiceProvider.GetRequiredService<IZeebeClient>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var jobHandlerInfo in jobHandlerInfoProvider.JobHandlerInfoCollection)
            {
                var worker = zeebeClient.NewWorker()
                    .JobType(jobHandlerInfo.JobType)
                    .Handler((jobClient, job) => HandleJob(jobClient, job, cancellationTokenSource.Token))
                    .FetchVariables(jobHandlerInfo.FetchVariabeles)
                    .MaxJobsActive(jobHandlerInfo.MaxJobsActive ?? zeebeWorkerOptions.MaxJobsActive)
                    .Name(zeebeWorkerOptions.Name ?? jobHandlerInfo.WorkerName)
                    .PollingTimeout(jobHandlerInfo.PollingTimeout ?? zeebeWorkerOptions.PollingTimeout)
                    .PollInterval(jobHandlerInfo.PollInterval ?? zeebeWorkerOptions.PollInterval)
                    .Timeout(jobHandlerInfo.Timeout ?? zeebeWorkerOptions.Timeout)
                    .Open();

                logger.LogInformation($"Created job worker to delegate job '{jobHandlerInfo.JobType}' to the boostrap job handler.");

                workers.Add(worker);
            }

            logger.LogInformation($"Created {workers.Count} job workers.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.cancellationTokenSource.Cancel();
            }
            finally
            {
                StopInternal();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopInternal();
            this.serviceScope.Dispose();
        }
        public void StopInternal()
        {
            workers.ForEach(w => w.Dispose());
            workers.Clear();
        }

        private Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var bootstrapJobHandler = scope.ServiceProvider.GetRequiredService<IBootstrapJobHandler>();
                return bootstrapJobHandler.HandleJob(jobClient, job, cancellationToken);
            }
        }
    }
}