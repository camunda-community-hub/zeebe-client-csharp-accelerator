using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class ExceptionJobHandler : IZeebeWorker
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public ExceptionJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            handleJobDelegate(job, cancellationToken);
            throw new BusinessException();
        }
    }

    public class BusinessException : BpmnErrorException
    {
        public BusinessException() : base("Exception", "Test") { }
    }
}