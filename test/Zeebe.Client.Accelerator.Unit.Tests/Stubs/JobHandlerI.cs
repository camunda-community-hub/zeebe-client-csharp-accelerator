using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobHandlerI : TypedInputHandlerBase<ExtendedJobHState>
    {
        public static IList<Guid> guids = new List<Guid>();

        protected override Task HandleJob(ExtendedJobHState variables, CancellationToken cancellationToken)
        {
            guids.Add(variables.Guid);
            return Task.CompletedTask;
        }
    }

    public class ExtendedJobHState : JobHState
    {
        public string ExtraAttr {  get; set; }
    }
}
