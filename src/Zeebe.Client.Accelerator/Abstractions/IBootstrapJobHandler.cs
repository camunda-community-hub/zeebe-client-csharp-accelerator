using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Abstractions
{
    /// <summary>
    /// The Job Handler Interface as required by the framework. Used internally.
    /// Not intended as generic worker interface.
    /// </summary>
    public interface IBootstrapJobHandler
    {
        Task HandleJob(IJobClient jobClient, IJob job, CancellationToken cancellationToken);
    }
}
