using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class InputJobHandler : IJobHandler<ZeebeJob<State>>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public InputJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(ZeebeJob<State> job, CancellationToken cancellationToken)
        {  
            handleJobDelegate(job, cancellationToken);
        }
    }

}