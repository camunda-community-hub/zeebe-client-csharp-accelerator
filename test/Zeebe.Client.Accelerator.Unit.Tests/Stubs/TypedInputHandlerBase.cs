using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public abstract class TypedInputHandlerBase<TInput> : IAsyncZeebeWorker<TInput> where TInput : JobHState, new ()
    {
        protected abstract Task HandleJob(TInput variables, CancellationToken cancellationToken);

        public async Task HandleJob(ZeebeJob<TInput> job, CancellationToken cancellationToken)
        {
            await HandleJob(job.getVariables(), cancellationToken);
        }
    }
}
