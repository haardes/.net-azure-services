namespace AzureServices.Core;

public class AzureServiceFactory : IAzureServiceFactory
{
    private IKeyVaultService? _keyVaultService;

    public IKeyVaultService KeyVaultService()
    {

        if (_keyVaultService == null)
        {
            AddKeyVaultService();
        }

        return _keyVaultService!;
    }

    public IAzureServiceFactory AddKeyVaultService(bool replace = false)
    {
        ThrowIfShouldNotReplace(_keyVaultService, replace);

        _keyVaultService = new KeyVaultService();
        return this;
    }

    public IAzureServiceFactory AddKeyVaultService(string? keyVaultUri, bool replace = false)
    {
        ThrowIfShouldNotReplace(_keyVaultService, replace);

        _keyVaultService = new KeyVaultService(keyVaultUri);
        return this;
    }

    private void ThrowIfShouldNotReplace<T>(T? instance, bool shouldReplace) where T : class
    {
        if (instance != null && !shouldReplace)
        {
            throw new Exception($"{instance.GetType().Name} already exists for this instance of {GetType().Name}. " +
                    $"To replace the current {instance.GetType().Name}, set parameter replace to true.");
        }
    }
}