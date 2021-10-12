using System;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobG : AbstractJob<JobGState>
    {
        public JobG(IJob job, JobGState state) : base(job, state) { }        
    }

    public class JobGState
    {
        public Guid Guid { get; set; }
        public bool Bool { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public double Double { get; set; }
    }
}