using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobA : AbstractJob
    {
        public JobA(IJob job) 
            : base(job)
        { }
    }
}