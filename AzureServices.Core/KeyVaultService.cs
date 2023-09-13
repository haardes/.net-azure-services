using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AzureServices.Core;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly Dictionary<string, string> _secretMap = new();

    public KeyVaultService() : this(Environment.GetEnvironmentVariable("KeyVaultUri")) { }

    public KeyVaultService(string? keyVaultUri)
    {
        if (string.IsNullOrEmpty(keyVaultUri))
        {
            throw new ArgumentNullException(nameof(keyVaultUri), $"No variable \"{nameof(keyVaultUri)}\" found. To create a parameterless {GetType().Name}, " +
                "make sure to add one in app.settings.json, as a configuration value, or set it as an environment variable.");
        }

        _secretClient = new(new(keyVaultUri), new DefaultAzureCredential());

        var secrets = _secretClient.GetPropertiesOfSecrets().ToList();

        foreach (var secret in secrets)
        {
            var name = secret.Name;
            var value = _secretClient.GetSecret(name).Value.Value;
            _secretMap.Add(name, value);
            Console.WriteLine($"Secret \"{name}\" added to {GetType().Name}.");
        }
    }

    public string GetSecret(string key)
    {
        return _secretMap.GetValueOrDefault(key, string.Empty);
    }
}