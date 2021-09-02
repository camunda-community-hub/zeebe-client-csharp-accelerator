using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
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