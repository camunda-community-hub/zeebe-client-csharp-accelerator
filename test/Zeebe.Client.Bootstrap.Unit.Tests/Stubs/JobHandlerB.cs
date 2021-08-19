using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobHandlerB : IAsyncJobHandler<JobC>
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerB(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
        public Task HandleJob(IJobClient client, JobC job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(client, job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
