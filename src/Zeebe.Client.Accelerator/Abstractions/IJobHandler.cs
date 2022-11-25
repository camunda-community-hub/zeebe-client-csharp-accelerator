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
}
