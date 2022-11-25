using System;
using System.Text.Json;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator
{
    public class ZeebeVariablesDeserializer : IZeebeVariablesDeserializer
    {
        private readonly JsonSerializerOptions options;

        public ZeebeVariablesDeserializer(JsonSerializerOptions options = null)
        {
            this.options = options ?? new JsonSerializerOptions();
            // ensure CamelCase naming
            this.options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, options);
        }

        public object Deserialize(string value, Type type)
        {
            return JsonSerializer.Deserialize(value, type, options);
        }
    }
}