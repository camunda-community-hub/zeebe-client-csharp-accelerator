using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleAsyncExample
{
    class SimpleVariableJobHandler : IAsyncJobHandler<SimpleVariableJob, SimpleVariableJobResponse>
    {
        public async Task<SimpleVariableJobResponse> HandleJob(SimpleVariableJob job, CancellationToken cancellationToken)
        {
            await Usecase.ExecuteAsync();
            return  new SimpleVariableJobResponse() 
            {
                Property = job.Key % 2 == 0
            };
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