using AzureServices.Core;

namespace AzureServices.Table;

public static class AzureServiceFactoryExtensions
{
    private static ITableService? _tableService;

    /// <summary>
    /// The <see cref="TableService"/> method returns an instance of <see cref="ITableService"/>. 
    /// If an <see cref="ITableService"/> has not previously been initialized in this <see cref="IAzureServiceFactory"/>, 
    /// the factory will try to initialize an instance with default values.
    /// The underlying method for initialization uses <see cref="AddKeyVaultService"/>. 
    /// </summary>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if an <see cref="ITableService"/> instance cannot be initialized with default values.</para>
    /// </remarks>
    /// <returns>An instance of <see cref="ITableService"/>.</returns>
    public static ref readonly ITableService TableService(this IAzureServiceFactory azureServiceFactory)
    {
        if (_tableService == null)
        {
            AddTableService(azureServiceFactory);
        }

        return ref _tableService!;
    }

    /// <summary>
    /// The <see cref="AddTableService"/> method initializes and registers an <see cref="ITableService"/> in this <see cref="IAzureServiceFactory"/>. This requires 
    /// a secret "StorageAccount" in the registered <see cref="IKeyVaultService"/>, and either a "StorageKey" or a "StorageConnectionString" secret.
    /// </summary>
    /// <param name="replace">Determines if an existing <see cref="ITableService"/> should be replaced if it already exists.</param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the registered <see cref="IKeyVaultService"/> (or the default initialization of it) cannot find a secret for "StorageAccount", and neither "StorageKey" or "StorageConnectionString".</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if default <see cref="IKeyVaultService"/> cannot find a secret for "StorageAccount", and neither "StorageKey" or "StorageConnectionString".</exception>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    public static IAzureServiceFactory AddTableService(this IAzureServiceFactory azureServiceFactory, bool replace = false)
    {
        azureServiceFactory.ThrowIfShouldNotReplace(_tableService, replace);

        _tableService = new TableService(azureServiceFactory.KeyOptions());
        return azureServiceFactory;
    }
}