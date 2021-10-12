using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Handlers
{
    public class InputJobHandler : IJobHandler<InputJob>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public InputJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(InputJob job, CancellationToken cancellationToken)
        {  
            handleJobDelegate(job, cancellationToken);
        }
    }

    public class InputJob : AbstractJob<State>
    {
        public InputJob(IJob job, State state) : base(job, state) { }
    }
}