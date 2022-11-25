using System;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [FetchVariables("100", "101", "102", "103", "104")]
    public class JobH : AbstractJob<JobHState>
    {
        public JobH(IJob job, JobHState state) : base(job, state) { }        
    }


    public class JobHState
    {
        public Guid Guid { get; set; }
    }
}