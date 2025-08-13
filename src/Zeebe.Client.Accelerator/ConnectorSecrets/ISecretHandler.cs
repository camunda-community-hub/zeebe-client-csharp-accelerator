using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public interface ISecretHandler
{
    Task<string> ReplaceSecretsAsync(string input);
}