using System.Threading;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using System.Text.Json.Serialization;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    public class InputJobHandler : IZeebeWorker<InputState>
    {
        private readonly HandleJobDelegate handleJobDelegate;

        public InputJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }

        public void HandleJob(ZeebeJob<InputState> job, CancellationToken cancellationToken)
        {  
            handleJobDelegate(job, cancellationToken);
        }
    }

    public class InputState : State
    {
        public string JsonPropertyNamedAttr { get; set; }
    }

}