using System.Threading;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IJobHandler<T>  where T : AbstractJob {
         void HandleJob(IJobClient client, T job, CancellationToken cancellationToken);
     }
}
