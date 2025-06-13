using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

/// <summary>
/// Interface for retrieving secrets from various sources
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Gets a secret value by key
    /// </summary>
    /// <param name="key">The secret key to retrieve</param>
    /// <returns>The secret value or null if not found</returns>
    public Task<string> GetSecretAsync(string key);
}