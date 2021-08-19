using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IAsyncJobHandler<T>  where T : AbstractJob {
        Task HandleJob(IJobClient client, T job, CancellationToken cancellationToken);
    }
}
