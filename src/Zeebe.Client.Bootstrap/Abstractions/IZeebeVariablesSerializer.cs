namespace Zeebe.Client.Bootstrap.Abstractions
{
    public interface IZeebeVariablesSerializer 
    {
        string Serialize(object value);
    }
}