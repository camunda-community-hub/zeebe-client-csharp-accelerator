using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Stubs
{
    public class SimpleJobHandler : IAsyncJobHandler<SimpleJob>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public SimpleJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(IJobClient client, SimpleJob job, CancellationToken cancellationToken)
        {  
            handleJobDelegate(client, job, cancellationToken);
            return client.NewCompleteJobCommand(job.Key).Send();
        }
    }

    public class SimpleJob : AbstractJob
    {
        public SimpleJob(IJob job) : base(job)
        { }
    }
}