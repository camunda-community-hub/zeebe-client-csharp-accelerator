using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleExample
{
    class SimpleJobHandler : IAsyncJobHandler<SimpleJob>
    {
        public Task HandleJob(IJobClient client, SimpleJob job, CancellationToken cancellationToken)
        {
            return client
                .NewCompleteJobCommand(job.Key)
                .Send(cancellationToken);
        }
    }

    class SimpleJob : AbstractJob
    {
        public SimpleJob(IJob job) : base(job)
        { }
    }
}