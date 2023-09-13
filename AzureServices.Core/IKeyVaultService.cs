using Azure.Security.KeyVault.Secrets;

namespace AzureServices.Core;

public interface IKeyVaultService
{
    /// <summary>
    /// Gets a secret from this <see cref="KeyVaultService"/>.
    /// </summary>
    /// <param name="key">the secret-name.</param>
    /// <returns>The secret <see cref="string"/> if the key is found, else <c>string.Empty</c></returns>
    string GetSecret(string key);
}