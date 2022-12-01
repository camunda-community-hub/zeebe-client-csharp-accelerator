using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerE : IAsyncZeebeWorker
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerE(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.FromException
            (
                new JobEException()
            );
        }
    }
}
