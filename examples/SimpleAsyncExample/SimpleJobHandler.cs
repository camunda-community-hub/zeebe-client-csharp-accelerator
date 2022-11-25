using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleAsyncExample
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