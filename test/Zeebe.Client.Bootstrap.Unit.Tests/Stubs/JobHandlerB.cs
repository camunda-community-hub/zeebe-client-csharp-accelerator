using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobHandlerB : IAsyncJobHandler<JobC>
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerB(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
        public Task HandleJob(JobC job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
