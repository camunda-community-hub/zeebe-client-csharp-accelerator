using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public delegate void HandleJobDelegate(IJobClient client, IJob job, CancellationToken cancellationToken);
}