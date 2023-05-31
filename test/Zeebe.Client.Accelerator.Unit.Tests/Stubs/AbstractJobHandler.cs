using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Attributes;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    [MaxJobsActive(int.MaxValue - 1)]
    [Timeout(int.MaxValue - 2)]
    [PollingTimeout(int.MaxValue - 3)]
    [PollInterval(int.MaxValue - 4)]
    public abstract class AbstractJobHandler
    {
        protected readonly HandleJobDelegate handleJobDelegate;

        public AbstractJobHandler(HandleJobDelegate handleJobDelegate)
        {
            this.handleJobDelegate = handleJobDelegate;
        }
    }
}
