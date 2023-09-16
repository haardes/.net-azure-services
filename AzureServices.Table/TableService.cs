using Azure.Data.Tables;
using AzureServices.Core;

namespace AzureServices.Table;

public class TableService : ITableService
{
    private readonly TableServiceClient _tableServiceClient;

    /// <summary>
    /// Calls <see cref="TableService(KeyOptions)"/> with <c>new KeyOptions()</c>. For more information on how this constructor works, see <see href="https://github.com/haardes/.net-azure-services"/>.
    /// </summary>
    public TableService() : this(new KeyOptions()) { }

    /// <summary>
    /// Creates an instance of <see cref="TableService"/> using the given <paramref name="keyOptions"/>. See <see href="https://github.com/haardes/.net-azure-services"/> for more information on how this constructor works.
    /// </summary>
    /// <param name="keyOptions"></param>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if no valid combination of variables are found as either environment variables or as KeyVault secrets.</para>
    /// </remarks>
    /// <exception cref="Exception">Thrown if no valid combination of variables are found as either environment variables or as KeyVault secrets.</exception>
    public TableService(KeyOptions keyOptions)
    {
        TryGetVariable(keyOptions.StorageAccount(), out string? storageAccount);
        TryGetVariable(keyOptions.StorageKey(), out string? storageKey);
        TryGetVariable(keyOptions.StorageConnectionString(), out string? connectionString);

        if (!string.IsNullOrEmpty(storageAccount) && !string.IsNullOrEmpty(storageKey))
        {
            _tableServiceClient = GetTableServiceClientFrom(storageAccount, storageKey);
        }
        else if (!string.IsNullOrEmpty(connectionString))
        {
            _tableServiceClient = GetTableServiceClientFrom(connectionString);
        }
        else
        {
            throw new Exception($"Error when registering TableServiceClient. Use another constructor or see https://github.com/haardes/.net-azure-services for details on how to use the parameterless BlobService() constructor.");
        }
    }

    public TableService(TableServiceClient blobServiceClient)
    {
        _tableServiceClient = blobServiceClient;
    }

    public TableService(string connectionString)
    {
        _tableServiceClient = GetTableServiceClientFrom(connectionString);
    }

    public TableService(string storageAccount, string storageKey)
    {
        _tableServiceClient = GetTableServiceClientFrom(storageAccount, storageKey);
    }

    public TableServiceClient GetServiceClient()
    {
        return _tableServiceClient;
    }

    private static void TryGetVariable(string key, out string? value)
    {
        value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrEmpty(value))
        {
            value = new AzureServiceFactory().KeyVaultService().GetSecret(key);
        }
    }

    private static TableServiceClient GetTableServiceClientFrom(string storage, string? key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            return new(storage);
        }

        return new(new($"https://{storage}.table.core.windows.net/"), new TableSharedKeyCredential(storage, key));
    }
}