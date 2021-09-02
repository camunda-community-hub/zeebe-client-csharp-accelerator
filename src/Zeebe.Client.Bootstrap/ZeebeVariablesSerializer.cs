using System.Text.Json;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap
{
    public class ZeebeVariablesSerializer : IZeebeVariablesSerializer
    {
        private readonly JsonSerializerOptions options;

        public ZeebeVariablesSerializer(JsonSerializerOptions options = null)
        {
            this.options = options;
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, options);
        }
    }
}