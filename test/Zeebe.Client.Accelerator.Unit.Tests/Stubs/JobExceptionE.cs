using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobEException : BpmnErrorException
    {
        public JobEException() 
            : base("12345", "54321")
        { }
    }
}