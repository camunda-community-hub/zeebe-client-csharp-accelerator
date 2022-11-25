using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobD : AbstractJob
    {
        public JobD(IJob job) 
            : base(job)
        { }
    }

    public class ResponseD
    { }
}