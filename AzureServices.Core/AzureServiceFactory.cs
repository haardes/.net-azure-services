namespace AzureServices.Core;
public class AzureServiceFactory : IAzureServiceFactory
{
    private IKeyVaultService? _keyVaultService;

    public IKeyVaultService KeyVaultService
    {
        get
        {
            if (_keyVaultService == null)
            {
                AddKeyVaultService();
            }

            return _keyVaultService;
        }
    }

    public IAzureServiceFactory AddKeyVaultService()
    {
        _keyVaultService = new KeyVaultService();
        return this;
    }

    public IAzureServiceFactory AddKeyVaultService(string? keyVaultUri)
    {
        _keyVaultService = new KeyVaultService(keyVaultUri);
        return this;
    }
}