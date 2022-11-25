using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerB : IAsyncJobHandler<JobC>, IAsyncJobHandler<JobD, ResponseD>, IAsyncJobHandler<JobE>, IJobHandler<JobF>, IAsyncJobHandler<JobG>, IAsyncJobHandler<JobH>, IAsyncJobHandler<ZeebeJob<ZeebeJobState>>
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

        public Task HandleJob(JobG job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }

        public Task HandleJob(JobH job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }

        public Task HandleJob(ZeebeJob<ZeebeJobState> job, CancellationToken cancellationToken)
        {
            this.handleJobDelegate(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
