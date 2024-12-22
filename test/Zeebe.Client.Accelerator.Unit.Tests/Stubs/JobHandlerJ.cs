using System.Threading;
using System.Threading.Tasks;

using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [TenantIds("tenant1", "tenant2")]
    public class JobHandlerJ : IAsyncZeebeWorker<JobHState>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerJ(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob<JobHState> job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}