using System.Threading;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Accelerator.Integration.Tests
{
    public delegate void HandleJobDelegate(IJob job, CancellationToken cancellationToken);
}