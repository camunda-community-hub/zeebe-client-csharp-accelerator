using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobA : AbstractJob
    {
        public JobA(IJob job) : base(job)
        { }
    }
}