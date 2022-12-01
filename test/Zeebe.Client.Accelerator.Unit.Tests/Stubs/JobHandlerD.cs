using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerD : IAsyncZeebeWorkerWithResult<ResponseD>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerD(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task<ResponseD> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.FromResult
            (
                new ResponseD()
            );
        }
    }
}
