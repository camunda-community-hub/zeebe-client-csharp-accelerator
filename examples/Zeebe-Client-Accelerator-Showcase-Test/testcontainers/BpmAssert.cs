using Io.Zeebe.Exporter.Proto;
using Io.Zeebe.Redis.Connect.Csharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PleaseWait.Dsl;
using static PleaseWait.TimeUnit;

namespace Zeebe_Client_Accelerator_Showcase_Test.testcontainers
{
    public class BpmAssert : IHostedService
    {
        public volatile List<ProcessInstanceRecord> processInstanceRecords = new();
        private readonly ILogger<BpmAssert>? _logger = null;

        public BpmAssert(ZeebeRedis zeebeRedis, ILoggerFactory? loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<BpmAssert>();

            zeebeRedis.AddProcessInstanceListener((record) => ReceiveProcessInstanceRecord(record));
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            processInstanceRecords.Clear();
            return Task.CompletedTask;
        }

        private void ReceiveProcessInstanceRecord(ProcessInstanceRecord record)
        {
            if (record.Metadata.RecordType.Equals(RecordMetadata.Types.RecordType.Event)) {
                processInstanceRecords.Add(record);
            }
        }

        public bool AssertThatProcessInstanceHasReachedElement(long processInstanceKey, String elementId)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.ElementId.Equals(elementId) &&
                pi.Metadata.Intent.Equals("ELEMENT_ACTIVATED")

            );
        }

        public void WaitUntilProcessInstanceHasReachedElement(long processInstanceKey, String elementId)
        {
            Wait().AtMost(10, SECONDS).PollInterval(1, SECONDS).Until(() => AssertThatProcessInstanceHasReachedElement(processInstanceKey, elementId));
        }

        public bool AssertThatProcessInstanceHasCompletedElement(long processInstanceKey, String elementId)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.ElementId.Equals(elementId) &&
                pi.Metadata.Intent.Equals("ELEMENT_COMPLETED")
            );
        }

        public void WaitUntilProcessInstanceHasCompletedElement(long processInstanceKey, String elementId)
        {
            Wait().AtMost(10, SECONDS).PollInterval(1, SECONDS).Until(() => AssertThatProcessInstanceHasCompletedElement(processInstanceKey, elementId));
        }
    }
}
