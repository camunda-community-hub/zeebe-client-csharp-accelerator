using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Stubs
{
    public class JobHandlerB : IAsyncJobHandler<JobC>, IAsyncJobHandler<JobD, ResponseD>, IAsyncJobHandler<JobE>, IJobHandler<JobF>
    {private readonly HandleJobDelegate handleJobDelegate;

        public JobHandlerB(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
        
        public Task HandleJob(JobC job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }

        public Task<ResponseD> HandleJob(JobD job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.FromResult
            (
                new ResponseD()
            );
        }

        public Task HandleJob(JobE job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.FromException
            (
                new JobEException()
            );
        }

        public void HandleJob(JobF job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            throw new Exception("123456789109876543210");
        }
    }
}
