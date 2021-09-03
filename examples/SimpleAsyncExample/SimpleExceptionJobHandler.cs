using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleAsyncExample
{
    class SimpleExceptionJobHandler : IAsyncJobHandler<SimpleExceptionJob>
    {
        public async Task HandleJob(SimpleExceptionJob job, CancellationToken cancellationToken)
        {
            if(job.Key % 2 == 0)
            {
                await Usecase.ExecuteAsync();
            }
            else
            {
                throw new SimpleJobException();
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