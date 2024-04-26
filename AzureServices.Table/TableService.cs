using System.Linq.Expressions;
using Azure;
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
        AzureServiceFactory.TryGetVariable(keyOptions.StorageAccount(), out string? storageAccount);
        AzureServiceFactory.TryGetVariable(keyOptions.StorageKey(), out string? storageKey);
        AzureServiceFactory.TryGetVariable(keyOptions.StorageConnectionString(), out string? connectionString);

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

    private static TableServiceClient GetTableServiceClientFrom(string storage, string? key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            return new(storage);
        }

        return new(new($"https://{storage}.table.core.windows.net/"), new TableSharedKeyCredential(storage, key));
    }

    public TableClient GetTable(string tableName, bool createIfNotExists = false)
    {
        TableClient table = _tableServiceClient.GetTableClient(tableName);

        if (createIfNotExists)
        {
            if (table.CreateIfNotExists() != null)
            {
                Console.WriteLine($"Table {tableName} created.");
            }
        }

        return table;
    }

    public static void AddEntityToTable<T>(TableClient table, T entity) where T : class, ITableEntity, new()
    {
        table.AddEntity(entity);
    }

    public void AddEntityToTable<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        TableClient table = GetTable(tableName);
        table.AddEntity(entity);
    }

    public static void AddEntitiesToTable<T>(TableClient table, IEnumerable<T> entities) where T : class, ITableEntity, new()
    {
        foreach (var entity in entities)
        {
            table.AddEntity(entity);
        }
    }

    public void AddEntitiesToTable<T>(string tableName, IEnumerable<T> entities) where T : class, ITableEntity, new()
    {
        TableClient table = GetTable(tableName);

        foreach (var entity in entities)
        {
            table.AddEntity(entity);
        }
    }

    public static List<T> QueryTable<T>(TableClient table, Expression<Func<T, bool>> query) where T : class, ITableEntity, new()
    {
        return table.Query(query).ToList();
    }

    public List<T> QueryTable<T>(string tableName, Expression<Func<T, bool>> query) where T : class, ITableEntity, new()
    {
        TableClient table = GetTable(tableName);

        return table.Query(query).ToList();
    }
}