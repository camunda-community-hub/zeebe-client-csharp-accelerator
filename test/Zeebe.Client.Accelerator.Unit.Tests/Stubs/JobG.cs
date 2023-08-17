using System;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Accelerator.Abstractions;
using System.Text.Json.Serialization;

namespace Zeebe.Client.Accelerator.Unit.Tests.Stubs
{
    public class JobGState
    {
        public Guid Guid { get; set; }
        public bool Bool { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }

        [JsonPropertyName("MY_double")]
        public double Double { get; set; }

        [JsonIgnore]
        public string ToBeIgnored { get; set; }

        public override bool Equals(object obj)
        {
            return obj is JobGState state &&
                   Guid.Equals(state.Guid) &&
                   Bool == state.Bool &&
                   Int == state.Int &&
                   DateTime == state.DateTime &&
                   String == state.String &&
                   Double == state.Double;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}