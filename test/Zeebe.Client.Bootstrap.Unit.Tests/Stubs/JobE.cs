using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobE : AbstractJob
    {
        public JobE(IJob job) 
            : base(job)
        { }
    }

    public class JobEException : AbstractJobException
    {
        public JobEException() 
            : base("12345", "54321")
        { }
    }
}