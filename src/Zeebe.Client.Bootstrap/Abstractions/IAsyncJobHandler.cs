using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IAsyncJobHandler
    {
        Task HandleJob(IJob job, CancellationToken cancellationToken);
    }

    public interface IAsyncJobHandler<Job>  where Job : AbstractJob
    {
        Task HandleJob(IJobClient client, Job job, CancellationToken cancellationToken);
    }

    public interface IAsyncJobHandler<Job, Response>  
        where Job : AbstractJob
        where Response : struct
    {
        Task<Response> HandleJob(IJobClient client, Job job, CancellationToken cancellationToken);
    }
}
