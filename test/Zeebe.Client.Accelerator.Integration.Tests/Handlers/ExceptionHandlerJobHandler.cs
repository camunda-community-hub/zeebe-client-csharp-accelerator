using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class ExceptionHandlerJobHandler : IZeebeWorker
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public ExceptionHandlerJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        
        public void HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        { 
            handleJobDelegate(job, cancellationToken);
        }
    }

}