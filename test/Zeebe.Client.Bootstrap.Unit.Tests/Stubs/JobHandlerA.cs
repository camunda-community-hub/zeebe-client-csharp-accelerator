using System.Threading;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public class JobHandlerA : IJobHandler<JobA>, IJobHandler<JobB>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerA(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(IJobClient client, JobA job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(client, job, cancellationToken);
        }

        public void HandleJob(IJobClient client, JobB job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(client, job, cancellationToken);
        }
}
}
