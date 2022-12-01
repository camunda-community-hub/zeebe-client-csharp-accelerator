using System;
using System.Threading;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerF : IZeebeWorker
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerF(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
        
        public void HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            throw new Exception("123456789109876543210");
        }

    }
}
