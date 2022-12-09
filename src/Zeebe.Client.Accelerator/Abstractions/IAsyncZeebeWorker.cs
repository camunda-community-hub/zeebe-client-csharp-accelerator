using System.Threading;
using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IAsyncZeebeWorker {
        Task HandleJob(ZeebeJob job, CancellationToken cancellationToken);
    }
    public interface IAsyncZeebeWorker<TInput> where TInput : class, new() {
        Task HandleJob(ZeebeJob<TInput> job, CancellationToken cancellationToken);

    }
    public interface IAsyncZeebeWorker<TInput, TResponse>
        where TInput : class, new ()
        where TResponse : class {
        Task<TResponse> HandleJob(ZeebeJob<TInput> job, CancellationToken cancellationToken);
    }
    public interface IAsyncZeebeWorkerWithResult<TResponse> where TResponse : class {
        Task<TResponse> HandleJob(ZeebeJob job, CancellationToken cancellationToken);
    }
}
