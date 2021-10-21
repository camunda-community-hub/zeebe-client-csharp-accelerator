using System;
using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap.Integration.Tests.Handlers
{
    public class OutputJobHandler : IJobHandler<OutputJob, State>
    {
        public static State State = new State()
        {
            Guid  = Guid.NewGuid(),
            Bool = new Random().Next(0, 1) == 1,
            Int = new Random().Next(),
            DateTime = DateTime.Now,
            String = Guid.NewGuid().ToString(),
            Double = new Random().NextDouble()
        };
        
        private readonly HandleJobDelegate handleJobDelegate;

        public OutputJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public State HandleJob(OutputJob job, CancellationToken cancellationToken)
        {
            State.Guid = job.State.Guid;
            handleJobDelegate(job, cancellationToken);
            return State;
        }
    }

    public class OutputJob : AbstractJob<State>
    {
        public OutputJob(IJob job, State state) : base(job, state) { }
    }

    public class State
    {
        public Guid Guid { get; set; }
        public bool Bool { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public double Double { get; set; }
    }
}