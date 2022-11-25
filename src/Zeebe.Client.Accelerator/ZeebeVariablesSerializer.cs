using System.Text.Json;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator
{
    public class ZeebeVariablesSerializer : IZeebeVariablesSerializer
    {
        private readonly JsonSerializerOptions options;

        public ZeebeVariablesSerializer(JsonSerializerOptions options = null)
        {
            this.options = options ?? new JsonSerializerOptions();
            // ensure CamelCase naming
            this.options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, options);
        }
    }
}