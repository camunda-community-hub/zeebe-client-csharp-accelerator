using System.Threading;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Accelerator.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    [JobType("TestJobType")]
    [MaxJobsActive(int.MaxValue - 1)]
    [Timeout(int.MaxValue - 2)]
    [PollingTimeout(int.MaxValue - 3)]
    [PollInterval(int.MaxValue - 4)]
    [FetchVariables("1", "2", "3", "4", "5")]
    [WorkerName("TestWorkerName")]
    public class JobHandlerA : IJobHandler<ZeebeJob>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerA(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
        }
    }
}
