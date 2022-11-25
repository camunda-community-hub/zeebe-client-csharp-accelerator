using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleExample
{
    class SimpleExceptionJobHandler : IJobHandler<SimpleExceptionJob>
    {
        public void HandleJob(SimpleExceptionJob job, CancellationToken cancellationToken)
        {
            if(job.Key % 2 == 0)
            {
                Usecase.Execute();
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

    class SimpleJobException : BpmnErrorException
    {
        public SimpleJobException() 
            : base("1", "A business exception has been thrown.")
        { }
    }
}