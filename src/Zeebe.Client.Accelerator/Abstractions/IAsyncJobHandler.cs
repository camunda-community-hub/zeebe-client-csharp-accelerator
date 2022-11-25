using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IZeebeJobHandler
    {
        Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken);
    }

    public interface IAsyncJobHandler<TJob>  where TJob : AbstractJob
    {
        Task HandleJob(TJob job, CancellationToken cancellationToken);
    }

    public interface IAsyncJobHandler<TJob, TResponse>  
        where TJob : AbstractJob
        where TResponse : class
    {
        Task<TResponse> HandleJob(TJob job, CancellationToken cancellationToken);
    }
}
