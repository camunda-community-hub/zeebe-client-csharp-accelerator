using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobF : AbstractJob
    {
        public JobF(IJob job) 
            : base(job)
        { }
    }
}