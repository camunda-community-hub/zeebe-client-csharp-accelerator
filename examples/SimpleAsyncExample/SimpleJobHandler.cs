using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleExample
{
    class SimpleJobHandler : IAsyncJobHandler<SimpleJob>
    {
        public async Task HandleJob(SimpleJob job, CancellationToken cancellationToken)
        {
            await Usecase.ExecuteAsync();
        }
    }

    class SimpleJob : AbstractJob
    {
        public SimpleJob(IJob job) : base(job)
        { }
    }
}