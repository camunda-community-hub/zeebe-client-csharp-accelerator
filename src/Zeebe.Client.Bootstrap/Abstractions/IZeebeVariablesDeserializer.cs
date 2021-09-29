namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IZeebeVariablesDeserializer 
    {
        T Deserialize<T>(string value);
    }
}