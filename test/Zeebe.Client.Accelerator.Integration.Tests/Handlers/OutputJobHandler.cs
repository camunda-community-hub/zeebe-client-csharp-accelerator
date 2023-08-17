using System;
using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using System.Text.Json.Serialization;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class OutputJobHandler : IZeebeWorker<State, OutputState>
    {
        public static OutputState State = new OutputState()
        {
            Guid = Guid.NewGuid(),
            Bool = new Random().Next(0, 1) == 1,
            Int = new Random().Next(),
            DateTime = DateTime.Now,
            String = Guid.NewGuid().ToString(),
            Double = new Random().NextDouble(),
            ToBeIgnored = Guid.NewGuid().ToString(),
            MyJsonPropertyName = "HelloJsonPropertyName",
        };

        private readonly HandleJobDelegate handleJobDelegate;

        public OutputJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public OutputState HandleJob(ZeebeJob<State> job, CancellationToken cancellationToken)
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

        [JsonIgnore]
        public string ToBeIgnored {  get; set; }
    }

    public class OutputState : State
    {
        [JsonPropertyName("jsonPropertyNamedAttr")]
        public string MyJsonPropertyName { get; set; }
    }
}