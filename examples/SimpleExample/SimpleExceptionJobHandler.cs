using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleExample
{
    class SimpleExceptionJobHandler : IAsyncJobHandler<SimpleExceptionJob>
    {
        public Task HandleJob(SimpleExceptionJob job, CancellationToken cancellationToken)
        {
            if(job.Key % 2 == 0)
            {
                return Task.CompletedTask;
            }
            else
            {
                return Task.FromException(new SimpleJobException());
            }
        }
    }

    class SimpleExceptionJob : AbstractJob
    {
        public SimpleExceptionJob(IJob job) : base(job)
        { }       
    }

    class SimpleJobException : AbstractJobException
    {
        public SimpleJobException() 
            : base("1", "A business exception has been thrown.")
        { }
    }
}