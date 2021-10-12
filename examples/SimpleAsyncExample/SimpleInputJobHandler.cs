using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleAsyncExample
{
    public class SimpleInputJobHandler : IAsyncJobHandler<SimpleInputJob>
    {
        public async Task HandleJob(SimpleInputJob job, CancellationToken cancellationToken)
        {
            await Usecase.ExecuteAsync();
        }
    }

    public class SimpleInputJob : AbstractJob<ProcessState>
    {
        public SimpleInputJob(IJob job, ProcessState state) : base(job, state) { }
    }
}