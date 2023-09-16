using Azure.Data.Tables;

namespace AzureServices.Table;

public interface ITableService
{
    /// <summary>
    /// The <see cref="GetServiceClient"/> method returns the underlying <see cref="TableServiceClient"/>.
    /// </summary>
    /// <returns>The underlying <see cref="TableServiceClient"/> instance.</returns>
    TableServiceClient GetServiceClient();
}