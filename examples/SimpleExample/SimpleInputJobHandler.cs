using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleExample
{
    public class SimpleInputJobHandler : IJobHandler<SimpleInputJob>
    {
        public void HandleJob(SimpleInputJob job, CancellationToken cancellationToken)
        {
            Usecase.Execute();
        }
    }

    public class SimpleInputJob : AbstractJob<ProcessState>
    {
        public SimpleInputJob(IJob job, ProcessState state) : base(job, state) { }
    }
}