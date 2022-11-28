using System;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{

    public class ZeebeJobState
    {
        public Guid ZeebeGuid { get; set; }
        public bool ZeebeBool { get; set; }
        public int ZeebeInt { get; set; }
        public DateTime ZeebeDateTime { get; set; }
        public string ZeebeString { get; set; }
        public double ZeebeDouble { get; set; }
    }
}