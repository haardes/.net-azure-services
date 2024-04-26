using Azure.Data.Tables;
using System.Linq.Expressions;

namespace AzureServices.Table;

public interface ITableService
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="entities"></param>
    void AddEntitiesToTable<T>(string tableName, IEnumerable<T> entities) where T : class, ITableEntity, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="entities"></param>
    static void AddEntitiesToTable<T>(TableClient table, IEnumerable<T> entities) where T : class, ITableEntity, new()
    {
        foreach (var entity in entities)
        {
            table.AddEntity(entity);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="entity"></param>
    void AddEntityToTable<T>(string tableName, T entity) where T : class, ITableEntity, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="entity"></param>
    static void AddEntityToTable<T>(TableClient table, T entity) where T : class, ITableEntity, new()
    {
        table.AddEntity(entity);
    }

    /// <summary>
    /// The <see cref="GetServiceClient"/> method returns the underlying <see cref="TableServiceClient"/>.
    /// </summary>
    /// <returns>The underlying <see cref="TableServiceClient"/> instance.</returns>
    TableServiceClient GetServiceClient();

    /// <summary>
    ///
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="createIfNotExists"></param>
    /// <returns></returns>
    TableClient GetTable(string tableName, bool createIfNotExists = false);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    List<T> QueryTable<T>(string tableName, Expression<Func<T, bool>> query) where T : class, ITableEntity, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    static List<T> QueryTable<T>(TableClient table, Expression<Func<T, bool>> query) where T : class, ITableEntity, new()
    {
        return table.Query(query).ToList();
    }
}