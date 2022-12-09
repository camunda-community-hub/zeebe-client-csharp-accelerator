using System;
using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class OutputJobHandler : IZeebeWorker<State, State>
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

        public State HandleJob(ZeebeJob<State> job, CancellationToken cancellationToken)
        {
            State.Guid = job.getVariables().Guid;
            handleJobDelegate(job, cancellationToken);
            return State;
        }
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