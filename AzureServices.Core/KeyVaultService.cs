using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AzureServices.Core;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly Dictionary<string, string> _secretMap = new();

    /// <summary>
    /// Creates an instance of a <see cref="KeyVaultService"/> using a KeyVaultUri variable in app.settings.json, 
    /// the configuration values, or the environment variables.
    /// </summary>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if a variable for "KeyVaultUri" is not found.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if a variable for "KeyVaultUri" is not found.</exception>
    public KeyVaultService() : this(Environment.GetEnvironmentVariable("KeyVaultUri")) { }

    /// <summary>
    /// Creates an instance of a <see cref="KeyVaultService"/> with the given keyVaultUri.
    /// </summary>
    /// <param name="keyVaultUri">the URI to connect to.</param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if keyVaultUri is <c>null</c> or empty.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if keyVaultUri is <c>null</c> or empty.</exception>
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