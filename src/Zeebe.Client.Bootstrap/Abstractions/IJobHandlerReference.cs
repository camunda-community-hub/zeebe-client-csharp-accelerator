using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IJobHandlerReference
    { 
        MethodInfo Handler { get; }
        ServiceLifetime HandlerServiceLifetime { get; }
        string JobType { get; }
        string WorkerName { get; }
        int? MaxJobsActive { get; }
        TimeSpan? Timeout { get; }
        TimeSpan? PollInterval { get; }
        TimeSpan? PollingTimeout { get; }
        string[] FetchVariabeles { get; }
    }
}