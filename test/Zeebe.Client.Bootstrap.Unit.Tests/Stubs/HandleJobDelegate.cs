using System.Threading;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public delegate void HandleJobDelegate(IJob job, CancellationToken cancellationToken);
}