using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    [WorkerName("TestWorkerName")]
    public class JobC : AbstractJob
    {
        public JobC(IJob job) 
            : base(job)
        { }
    }
}