using Azure.Security.KeyVault.Secrets;

namespace AzureServices.Core;

public interface IKeyVaultService
{
    string GetSecret(string key);
}