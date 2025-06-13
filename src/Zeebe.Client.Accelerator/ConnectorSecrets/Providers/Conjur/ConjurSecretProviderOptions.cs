namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.Conjur;

public class ConjurSecretProviderOptions
{
    public string ApiUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string SecretPath { get; set; }
}