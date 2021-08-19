using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    [JobType("TestJobType")]
    public class JobB : AbstractJob
    {
        public JobB(IJob job) : base(job)
        { }
    }
}