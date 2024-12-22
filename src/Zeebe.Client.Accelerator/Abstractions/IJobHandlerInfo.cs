using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IJobHandlerInfo
    {
        MethodInfo Handler { get; }
        ServiceLifetime HandlerServiceLifetime { get; }
        string JobType { get; }
        string WorkerName { get; }
        int? MaxJobsActive { get; }
        byte? HandlerThreads { get; }
        TimeSpan? Timeout { get; }
        TimeSpan? PollInterval { get; }
        TimeSpan? PollingTimeout { get; }
        string[] FetchVariabeles { get; }
        bool AutoComplete { get; }
        public string[] TenantIds { get; }
    }
}