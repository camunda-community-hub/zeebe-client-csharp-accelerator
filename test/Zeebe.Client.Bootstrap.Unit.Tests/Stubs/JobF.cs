using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobF : AbstractJob
    {
        public JobF(IJob job) 
            : base(job)
        { }
    }
}