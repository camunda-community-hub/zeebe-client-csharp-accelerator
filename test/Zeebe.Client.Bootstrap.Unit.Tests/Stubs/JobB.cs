using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Attributes;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    [JobType("TestJobType")]
    [MaxJobsActive(int.MaxValue -1)]
    [Timeout(int.MaxValue - 2)]
    [PollingTimeout(int.MaxValue - 3)]
    [PollInterval(int.MaxValue - 4)]
    public class JobB : AbstractJob
    {
        public JobB(IJob job) : base(job)
        { }
    }
}