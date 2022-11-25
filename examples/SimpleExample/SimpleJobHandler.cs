using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleExample
{
    class SimpleJobHandler : IJobHandler<SimpleJob>
    {
        public void HandleJob(SimpleJob job, CancellationToken cancellationToken)
        {
            Usecase.Execute();
        }
    }

    class SimpleJob : AbstractJob
    {
        public SimpleJob(IJob job) : base(job)
        { }
    }
}