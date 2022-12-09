using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class SimpleJobHandler : IAsyncZeebeWorker
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public SimpleJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {  
            handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }

}