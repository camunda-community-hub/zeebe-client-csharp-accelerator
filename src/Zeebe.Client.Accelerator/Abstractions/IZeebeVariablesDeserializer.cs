using System;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IZeebeVariablesDeserializer 
    {
        T Deserialize<T>(string value);
        object Deserialize(string value, Type type);
    }
}