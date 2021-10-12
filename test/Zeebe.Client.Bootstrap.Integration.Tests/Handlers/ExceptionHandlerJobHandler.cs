using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Handlers
{
    public class ExceptionHandlerJobHandler : IJobHandler<ExceptionHandlerJob>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public ExceptionHandlerJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        
        public void HandleJob(ExceptionHandlerJob job, CancellationToken cancellationToken)
        { 
            handleJobDelegate(job, cancellationToken);
        }
    }

    public class ExceptionHandlerJob : AbstractJob
    {
        public ExceptionHandlerJob(IJob job) : base(job) { }
    }
}