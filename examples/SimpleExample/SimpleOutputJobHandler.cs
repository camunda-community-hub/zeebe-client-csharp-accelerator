using System;
using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace SimpleExample
{
    public class SimpleOutputJobHandler : IJobHandler<SimpleOutputJob, ProcessState>
    {
        public ProcessState HandleJob(SimpleOutputJob job, CancellationToken cancellationToken)
        {
            var random = new Random();

            Usecase.Execute();
            
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