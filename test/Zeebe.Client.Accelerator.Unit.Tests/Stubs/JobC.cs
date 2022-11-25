using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [WorkerName("TestWorkerName")]
    public class JobC : AbstractJob
    {
        public JobC(IJob job) 
            : base(job)
        { }
    }
}