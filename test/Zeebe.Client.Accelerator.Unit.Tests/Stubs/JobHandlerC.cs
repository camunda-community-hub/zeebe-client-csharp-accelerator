using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [FetchVariables(none: true)]
    public class JobHandlerC : IAsyncZeebeWorker
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerC(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
        
        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }

    }
}
