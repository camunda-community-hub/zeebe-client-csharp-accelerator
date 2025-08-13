using System.Collections.Generic;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public class SecretOptions
{
    public const string Section = "ConnectorSecrets";
    public List<string> Providers { get; set; } = new();
}