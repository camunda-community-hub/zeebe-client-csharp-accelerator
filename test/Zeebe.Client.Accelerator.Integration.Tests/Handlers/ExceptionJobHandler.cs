using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class ExceptionJobHandler : IJobHandler<ExceptionJob>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public ExceptionJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(ExceptionJob job, CancellationToken cancellationToken)
        {
            handleJobDelegate(job, cancellationToken);
            throw new BusinessException();
        }
    }

    public class ExceptionJob : AbstractJob
    {
        public ExceptionJob(IJob job) : base(job) { }
    }

    public class BusinessException : BpmnErrorException
    {
        public BusinessException() : base("Exception", "Test") { }
    }
}