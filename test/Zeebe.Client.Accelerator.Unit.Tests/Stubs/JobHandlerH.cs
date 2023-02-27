using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{

    [FetchVariables("Var100", "var101", "Variable_102", "variable_103", "Different_thing_104")]
    public class JobHandlerH :IAsyncZeebeWorker<JobHState>
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerH(HandleJobDelegate handleJobDelegate)
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
