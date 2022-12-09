using System.Threading;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IZeebeWorker {
        void HandleJob(ZeebeJob job, CancellationToken cancellationToken);
    }
    public interface IZeebeWorker<TInput> where TInput : class, new() {
        void HandleJob(ZeebeJob<TInput> job, CancellationToken cancellationToken);
    }
    public interface IZeebeWorker<TInput, TResponse>
        where TInput : class, new()
        where TResponse : class
    {
        TResponse HandleJob(ZeebeJob<TInput> job, CancellationToken cancellationToken);
    }
    public interface IZeebeWorkerWithResult<TResponse> where TResponse : class {
        TResponse HandleJob(ZeebeJob job, CancellationToken cancellationToken);
    }
}
