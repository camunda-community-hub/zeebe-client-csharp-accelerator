using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

namespace Zeebe.Client.Accelerator.Abstractions
{

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


    public interface IAsyncZeebeWorker : IAsyncJobHandler<ZeebeJob> { }
    public interface IAsyncZeebeWorker<TInput> : IAsyncJobHandler<ZeebeJob<TInput>> where TInput : class, new() { }
    public interface IAsyncZeebeWorker<TInput, TResponse> : IAsyncJobHandler<ZeebeJob<TInput>,TResponse> 
        where TInput : class, new ()
        where TResponse : class { }
    public interface IAsyncZeebeWorkerWithResult<TResponse> : IAsyncJobHandler<ZeebeJob, TResponse> where TResponse : class { }
}
