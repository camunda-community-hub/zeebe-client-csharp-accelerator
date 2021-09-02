using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleExample
{
    class SimpleVariableJobHandler : IAsyncJobHandler<SimpleVariableJob, SimpleVariableJobResponse>
    {
        public Task<SimpleVariableJobResponse> HandleJob(SimpleVariableJob job, CancellationToken cancellationToken)
        {
            return Task.FromResult
            (
                new SimpleVariableJobResponse() 
                {
                    Property = job.Key % 2 == 0
                }
            );
        }
    }

    class SimpleVariableJob : AbstractJob
    {
        public SimpleVariableJob(IJob job) : base(job)
        { }
    }

    class SimpleVariableJobResponse 
    {
        public bool Property { get; set; }
    }
}