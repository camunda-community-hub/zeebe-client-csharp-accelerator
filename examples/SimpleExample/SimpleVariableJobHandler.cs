using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleExample
{
    class SimpleVariableJobHandler : IJobHandler<SimpleVariableJob, SimpleVariableJobResponse>
    {
        public SimpleVariableJobResponse HandleJob(SimpleVariableJob job, CancellationToken cancellationToken)
        {
            Usecase.Execute();
            return new SimpleVariableJobResponse() 
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