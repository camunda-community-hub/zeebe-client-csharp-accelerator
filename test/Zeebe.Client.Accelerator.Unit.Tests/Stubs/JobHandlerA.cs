using System.Threading;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Accelerator.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [JobType("TestJobType")]
    [FetchVariables("1", "2", "3", "4", "5")]
    [WorkerName("TestWorkerName")]
    public class JobHandlerA : AbstractJobHandler, IZeebeWorker
    {

        public JobHandlerA(HandleJobDelegate handleJobDelegate) : base(handleJobDelegate) { }

        public void HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
        }
    }
}
