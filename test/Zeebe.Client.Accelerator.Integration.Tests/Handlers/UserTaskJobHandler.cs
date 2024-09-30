using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Integration.Tests.Handlers
{
    [JobType("io.camunda.zeebe:userTask")]
    [AutoComplete(false)]
    [Timeout(2592000000)]
    public class UserTaskJobHandler : IAsyncZeebeWorker
    {

        private readonly HandleJobDelegate _handleJobDelegate;

        public UserTaskJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this._handleJobDelegate = handleJobDelegate;
        }

        public Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
        {
            _handleJobDelegate.Invoke(job, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
