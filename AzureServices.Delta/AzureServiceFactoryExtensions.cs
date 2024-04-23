using AzureServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServices.Delta;

public static class AzureServiceFactoryExtensions
{
    private static IDeltaService? _deltaService;

    /// <summary>
    /// The <see cref="DeltaService(IAzureServiceFactory)"/> method returns an instance of <see cref="IDeltaService"/>. 
    /// If an <see cref="IDeltaService"/> has not previously been initialized in this <see cref="IAzureServiceFactory"/>, 
    /// the factory will try to initialize an instance with default values.
    /// The underlying method for initialization uses <see cref="AddDeltaService"/>. 
    /// </summary>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if an <see cref="IDeltaService"/> instance cannot be initialized with default values.</para>
    /// </remarks>
    /// <returns>An instance of <see cref="IDeltaService"/>.</returns>
    public static ref readonly IDeltaService DeltaService(this IAzureServiceFactory azureServiceFactory)
    {
        if (_deltaService == null)
        {
            AddDeltaService(azureServiceFactory);
        }

        return ref _deltaService!;
    }

    /// <summary>
    /// The <see cref="AddDeltaService(IAzureServiceFactory, bool)"/> method initializes and registers an <see cref="IDeltaService"/> in this <see cref="IAzureServiceFactory"/>. This requires 
    /// secrets "DatabricksWorkspaceId", "DatabricksWarehouseId" and "DatabricksApiToken" to be present in the registered <see cref="IKeyVaultService"/>.
    /// </summary>
    /// <param name="replace">Determines if an existing <see cref="IDeltaService"/> should be replaced if it already exists.</param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the registered <see cref="IKeyVaultService"/> (or the default initialization of it) cannot find a secret for "DatabricksWorkspaceId", "DatabricksWarehouseId" and "DatabricksApiToken".</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if default <see cref="IKeyVaultService"/> cannot find a secret for "DatabricksWorkspaceId", "DatabricksWarehouseId" and "DatabricksApiToken".</exception>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    public static IAzureServiceFactory AddDeltaService(this IAzureServiceFactory azureServiceFactory, bool replace = false)
    {
        azureServiceFactory.ThrowIfShouldNotReplace(_deltaService, replace);

        _deltaService = new DeltaService(azureServiceFactory.KeyVaultService(), azureServiceFactory.KeyOptions());
        return azureServiceFactory;
    }
}