using System.Threading;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IJobHandler<TJob>  where TJob : AbstractJob
    {
         void HandleJob(TJob job, CancellationToken cancellationToken);
    }

    public interface IJobHandler<TJob, TResponse>  
        where TJob : AbstractJob
        where TResponse : class
    {
         TResponse HandleJob(TJob job, CancellationToken cancellationToken);
    }

    public interface IZeebeWorker : IJobHandler<ZeebeJob> { }
    public interface IZeebeWorker<TInput> : IJobHandler<ZeebeJob<TInput>> where TInput : class, new() { }
    public interface IZeebeWorker<TInput, TResponse> : IJobHandler<ZeebeJob<TInput>, TResponse>
        where TInput : class, new()
        where TResponse : class
    { }
    public interface IZeebeWorkerWithResult<TResponse> : IJobHandler<ZeebeJob, TResponse> where TResponse : class { }
}
