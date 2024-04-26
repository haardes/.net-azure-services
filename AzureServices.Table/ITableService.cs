using Azure.Data.Tables;
using System.Linq.Expressions;

namespace AzureServices.Table;

public interface ITableService
{
    void AddEntitiesToTable<T>(string tableName, IEnumerable<T> entities) where T : class, ITableEntity, new();

    static void AddEntitiesToTable<T>(TableClient table, IEnumerable<T> entities) where T : class, ITableEntity, new()
    {
        foreach (var entity in entities)
        {
            table.AddEntity(entity);
        }
    }

    void AddEntityToTable<T>(string tableName, T entity) where T : class, ITableEntity, new();

    static void AddEntityToTable<T>(TableClient table, T entity) where T : class, ITableEntity, new()
    {
        table.AddEntity(entity);
    }

    TableServiceClient GetServiceClient();

    TableClient GetTable(string tableName, bool createIfNotExists = false);

    List<T> QueryTable<T>(string tableName, Expression<Func<T, bool>> query) where T : class, ITableEntity, new();

    static List<T> QueryTable<T>(TableClient table, Expression<Func<T, bool>> query) where T : class, ITableEntity, new()
    {
        return table.Query(query).ToList();
    }
}