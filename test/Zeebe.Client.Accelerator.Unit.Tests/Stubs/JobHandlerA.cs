using System.Threading;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Accelerator.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public class JobHandlerA : IJobHandler<JobA>, IJobHandler<JobB>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerA(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(JobA job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
        }

        public void HandleJob(JobB job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
        }
}
}
