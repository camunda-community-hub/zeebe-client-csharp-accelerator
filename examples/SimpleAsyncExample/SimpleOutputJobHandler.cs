using System;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace SimpleAsyncExample
{
    public class SimpleOutputJobHandler : IAsyncJobHandler<SimpleOutputJob, ProcessState>
    {
        public async Task<ProcessState> HandleJob(SimpleOutputJob job, CancellationToken cancellationToken)
        {
            var random = new Random();

            await Usecase.ExecuteAsync();
            
            return new ProcessState()
            {
                Guid  = Guid.NewGuid(),
                Bool = random.Next(0, 1) == 1,
                Int = random.Next(),
                DateTime = DateTime.Now,
                String = Guid.NewGuid().ToString(),
                Double = random.NextDouble()
            };
        }   
    }

    public class SimpleOutputJob : AbstractJob
    {
        public SimpleOutputJob(IJob job) : base(job) { }
    }

    public class ProcessState
    {
        public Guid Guid { get; set; }
        public bool Bool { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public double Double { get; set; }
    }
}