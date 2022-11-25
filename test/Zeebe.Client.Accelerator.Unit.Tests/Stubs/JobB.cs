using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [JobType("TestJobType")]
    [MaxJobsActive(int.MaxValue -1)]
    [Timeout(int.MaxValue - 2)]
    [PollingTimeout(int.MaxValue - 3)]
    [PollInterval(int.MaxValue - 4)]
    [FetchVariables("1", "2", "3", "4", "5")]
    public class JobB : AbstractJob
    {
        public JobB(IJob job) 
            : base(job)
        { }
    }
}