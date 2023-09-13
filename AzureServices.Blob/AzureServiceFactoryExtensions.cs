using AzureServices.Core;

namespace AzureServices.Blob;

public static class AzureServiceFactoryExtensions
{
    private static IBlobService? _blobService;

    /// <summary>
    /// The <see cref="BlobService"/> method returns an instance of <see cref="IBlobService"/>. 
    /// If an <see cref="IBlobService"/> has not previously been initialized in this <see cref="IAzureServiceFactory"/>, 
    /// the factory will try to initialize an instance with default values.
    /// The underlying method for initialization uses <see cref="AddKeyVaultService"/>. 
    /// </summary>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if an <see cref="IBlobService"/> instance cannot be initialized with default values.</para>
    /// </remarks>
    /// <returns>An instance of <see cref="IBlobService"/>.</returns>
    public static ref readonly IBlobService BlobService(this IAzureServiceFactory azureServiceFactory)
    {
        return ref _blobService;
    }

    /// <summary>
    /// The <see cref="AddBlobService"/> method initializes and registers an <see cref="IBlobService"/> in this <see cref="IAzureServiceFactory"/>. This requires 
    /// a secret "StorageAccount" in the registered <see cref="IKeyVaultService"/>, and either a "StorageKey" or a "StorageConnectionString" secret.
    /// </summary>
    /// <param name="replace">Determines if an existing <see cref="IBlobService"/> should be replaced if it already exists.</param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the registered <see cref="IKeyVaultService"/> (or the default initialization of it) cannot find a secret for "StorageAccount", and neither "StorageKey" or "StorageConnectionString".</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if default <see cref="IKeyVaultService"/> cannot find a secret for "StorageAccount", and neither "StorageKey" or "StorageConnectionString".</exception>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    public static IAzureServiceFactory AddBlobService(this IAzureServiceFactory azureServiceFactory, bool replace = false)
    {
        azureServiceFactory.ThrowIfShouldNotReplace(_blobService, replace);

        _blobService = new BlobService();
        return azureServiceFactory;
    }
}