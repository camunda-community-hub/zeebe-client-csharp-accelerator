using System.Text.Json;
using Zeebe.Client.Bootstrap.Abstractions;

namespace Zeebe.Client.Bootstrap
{
    public class ZeebeVariablesDeserializer : IZeebeVariablesDeserializer
    {
        private readonly JsonSerializerOptions options;

        public ZeebeVariablesDeserializer(JsonSerializerOptions options = null)
        {
            this.options = options;
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}