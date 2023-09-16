using AzureServices.Core;

namespace AzureServices.Table;

public static class KeyOptionsExtensions
{
    private static string _storageAccount = "StorageAccount";
    private static string _storageKey = "StorageKey";
    private static string _storageConnectionString = "StorageConnectionString";

    public static ref readonly string StorageAccount(this KeyOptions keyOptions)
    {
        return ref _storageAccount;
    }

    public static KeyOptions StorageAccount(this KeyOptions keyOptions, string keyName)
    {
        _storageAccount = keyName;
        return keyOptions;
    }

    public static ref readonly string StorageKey(this KeyOptions keyOptions)
    {
        return ref _storageKey;
    }

    public static KeyOptions StorageKey(this KeyOptions keyOptions, string keyName)
    {
        _storageKey = keyName;
        return keyOptions;
    }

    public static ref readonly string StorageConnectionString(this KeyOptions keyOptions)
    {
        return ref _storageConnectionString;
    }

    public static KeyOptions StorageConnectionString(this KeyOptions keyOptions, string keyName)
    {
        _storageConnectionString = keyName;
        return keyOptions;
    }
}