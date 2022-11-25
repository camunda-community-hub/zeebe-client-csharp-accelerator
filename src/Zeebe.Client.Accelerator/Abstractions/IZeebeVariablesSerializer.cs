namespace Zeebe.Client.Accelerator.Abstractions
{
    public interface IZeebeVariablesSerializer 
    {
        string Serialize(object value);
    }
}