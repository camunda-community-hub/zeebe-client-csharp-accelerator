using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class SimpleJobException : AbstractJobException
    {
        public SimpleJobException(string code, string message) 
        : base(code, message)
        { }
    }
}