using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IBootstrapJobHandler
    {
        Task HandleJob(IJob job, CancellationToken cancellationToken);
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
