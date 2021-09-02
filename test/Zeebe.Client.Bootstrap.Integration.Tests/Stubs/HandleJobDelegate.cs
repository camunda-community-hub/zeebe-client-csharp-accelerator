using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Stubs
{
    public delegate void HandleJobDelegate(IJob job, CancellationToken cancellationToken);
}