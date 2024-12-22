using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator
{
    public class JobHandlerInfo : IJobHandlerInfo
    {
        public JobHandlerInfo(
            MethodInfo handler,
            ServiceLifetime handlerServiceLifetime,
            string jobType,
            string workerName,
            int? maxJobsActive = null,
            byte? handlerThreads = null,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            TimeSpan? pollingTimeout = null,
            string[] fetchVariabeles = null,
            bool? autoComplete = null,
            string[] tenantIds = null)
        {
            if (string.IsNullOrWhiteSpace(jobType))
                throw new ArgumentException($"'{nameof(jobType)}' cannot be null or whitespace.", nameof(jobType));

            if (string.IsNullOrWhiteSpace(workerName))
                throw new ArgumentException($"'{nameof(workerName)}' cannot be null or whitespace.", nameof(workerName));

            if (maxJobsActive.HasValue && maxJobsActive.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(maxJobsActive));
            if (handlerThreads.HasValue && handlerThreads.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(handlerThreads));
            if (timeout.HasValue && timeout.Value.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(timeout));
            if (pollInterval.HasValue && pollInterval.Value.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(pollInterval));
            if (pollingTimeout.HasValue && pollingTimeout.Value.TotalMilliseconds < 1)
                throw new ArgumentOutOfRangeException(nameof(pollingTimeout));

            this.HandlerServiceLifetime = handlerServiceLifetime;
            this.Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.JobType = jobType;
            this.WorkerName = workerName;
            this.MaxJobsActive = maxJobsActive;
            this.HandlerThreads = handlerThreads;
            this.Timeout = timeout;
            this.PollInterval = pollInterval;
            this.PollingTimeout = pollingTimeout;
            this.FetchVariabeles = fetchVariabeles ?? (new string[0]);
            this.AutoComplete = autoComplete ?? true;
            this.TenantIds = tenantIds ?? Array.Empty<string>();
        }

        public MethodInfo Handler { get; }
        public ServiceLifetime HandlerServiceLifetime { get; }
        public string JobType { get; }
        public string WorkerName { get; }
        public int? MaxJobsActive { get; }
        public byte? HandlerThreads { get; }
        public TimeSpan? Timeout { get; }
        public TimeSpan? PollInterval { get; }
        public TimeSpan? PollingTimeout { get; }
        public string[] FetchVariabeles { get; }
        public bool AutoComplete { get; }
        public string[] TenantIds { get; }
    }
}