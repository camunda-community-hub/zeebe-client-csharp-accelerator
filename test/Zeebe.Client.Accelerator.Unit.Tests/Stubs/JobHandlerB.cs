using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerB : IAsyncZeebeWorker<ZeebeJobState>
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerB(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob<ZeebeJobState> job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
