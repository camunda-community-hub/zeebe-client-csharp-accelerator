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

        private static int WAIT_SECONDS = 7;
        private static int POLL_MILLIS = 500;

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
            processInstanceRecords.Add(record);
            _logger?.LogTrace("{}", record);
        }

        // ----------------------------------------------------------------------------------------------
        // Assertions below

        private bool CheckThatProcessInstanceHasReachedElement(long processInstanceKey, String elementId)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.ElementId.Equals(elementId) &&
                pi.Metadata.Intent.Equals("ELEMENT_ACTIVATED")

            );
        }

        public void AssertThatProcessInstanceHasReachedElement(long processInstanceKey, String elementId)
        {
            Assert.True(CheckThatProcessInstanceHasReachedElement(processInstanceKey, elementId));
        }

        public void WaitUntilProcessInstanceHasReachedElement(long processInstanceKey, String elementId)
        {
            Wait().AtMost(WAIT_SECONDS, Seconds).PollInterval(POLL_MILLIS, Millis).Until(() => CheckThatProcessInstanceHasReachedElement(processInstanceKey, elementId));
        }

        // --------------------

        private bool CheckThatProcessInstanceHasCompletedElement(long processInstanceKey, String elementId)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.ElementId.Equals(elementId) &&
                pi.Metadata.Intent.Equals("ELEMENT_COMPLETED")
            );
        }

        public void AssertThatProcessInstanceHasCompletedElement(long processInstanceKey, String elementId)
        {
            Assert.True(CheckThatProcessInstanceHasCompletedElement(processInstanceKey, elementId));
        }

        public void WaitUntilProcessInstanceHasCompletedElement(long processInstanceKey, String elementId)
        {
            Wait().AtMost(WAIT_SECONDS, Seconds).PollInterval(POLL_MILLIS, Millis).Until(() => CheckThatProcessInstanceHasCompletedElement(processInstanceKey, elementId));
        }

        // --------------------

        private bool CheckThatProcessInstanceHasStarted(long processInstanceKey)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.BpmnElementType.Equals("PROCESS") &&
                pi.Metadata.Intent.Equals("ELEMENT_ACTIVATED")
            );
        }

        public void AssertThatProcessInstanceHasStarted(long processInstanceKey)
        {
            Assert.True(CheckThatProcessInstanceHasStarted(processInstanceKey));
        }

        public void WaitUntilProcessInstanceHasStarted(long processInstanceKey)
        {
            Wait().AtMost(WAIT_SECONDS, Seconds).PollInterval(POLL_MILLIS, Millis).Until(() => CheckThatProcessInstanceHasStarted(processInstanceKey));
        }

        // --------------------

        private bool CheckThatProcessInstanceHasEnded(long processInstanceKey)
        {
            return processInstanceRecords.Exists(pi =>
                pi.ProcessInstanceKey.Equals(processInstanceKey) &&
                pi.BpmnElementType.Equals("PROCESS") &&
                pi.Metadata.Intent.Equals("ELEMENT_COMPLETED")
            );
        }

        public void AssertThatProcessInstanceHasEnded(long processInstanceKey)
        {
            Assert.True(CheckThatProcessInstanceHasEnded(processInstanceKey));
        }

        public void WaitUntilProcessInstanceHasEnded(long processInstanceKey)
        {
            Wait().AtMost(WAIT_SECONDS, Seconds).PollInterval(POLL_MILLIS, Millis).Until(() => CheckThatProcessInstanceHasEnded(processInstanceKey));
        }
    }
}
