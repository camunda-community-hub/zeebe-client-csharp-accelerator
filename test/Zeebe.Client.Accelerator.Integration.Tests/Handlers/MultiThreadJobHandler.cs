using System;
using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    [MaxJobsActive(120)]
    [HandlerThreads(8)]
    public class MultiThreadJobHandler : IAsyncZeebeWorkerWithResult<Output>
    {
       
        private readonly HandleJobDelegate handleJobDelegate;

        public MultiThreadJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public Task<Output> HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            int currentThreadId = Environment.CurrentManagedThreadId;
            handleJobDelegate.Invoke(job, cancellationToken);
            return Task.FromResult(new Output
            {
                ThreadId = currentThreadId
            });
        }
    }

    public class Output
    {
        public int ThreadId { get; set; }
    }
}