using System.Threading;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IJobHandler<Job>  where Job : AbstractJob
    {
         void HandleJob(IJobClient client, Job job, CancellationToken cancellationToken);
    }

    public interface IJobHandler<Job, Response>  
        where Job : AbstractJob
        where Response : struct
    {
         Response HandleJob(IJobClient client, Job job, CancellationToken cancellationToken);
    }
}
