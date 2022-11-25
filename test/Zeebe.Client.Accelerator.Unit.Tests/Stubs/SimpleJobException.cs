using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class SimpleJobException : BpmnErrorException
    {
        public SimpleJobException(string code, string message) 
        : base(code, message)
        { }
    }
}