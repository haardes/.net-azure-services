namespace AzureServices.Core;

public class AzureServiceFactory : IAzureServiceFactory
{
    public static KeyOptions _keyOptions = new();
    private static IKeyVaultService? _keyVaultService;

    public KeyOptions KeyOptions()
    {
        return _keyOptions;
    }

    public IKeyVaultService KeyVaultService()
    {

        if (_keyVaultService == null)
        {
            AddKeyVaultService();
        }

        return _keyVaultService!;
    }

    public AzureServiceFactory() { }

    public AzureServiceFactory(KeyOptions keyOptions)
    {
        _keyOptions = keyOptions;
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

    public void ThrowIfShouldNotReplace<T>(T? instance, bool shouldReplace) where T : class
    {
        if (instance != null && !shouldReplace)
        {
            throw new Exception($"{instance.GetType().Name} already exists for this instance of {GetType().Name}. " +
                    $"To replace the current {instance.GetType().Name}, set parameter replace to true.");
        }
    }

    public static void TryGetVariable(string key, out string? value)
    {
        value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrEmpty(value))
        {
            if (_keyVaultService == null)
            {
                throw new Exception("KeyVaultService not yet initialized.");
            }

            value = _keyVaultService.GetSecret(key);
        }
    }
}