namespace AzureServices.Core;

public interface IAzureServiceFactory
{
    /// <summary>
    /// The <see cref="KeyVaultService"/> attribute returns an instance of <see cref="IKeyVaultService"/>. 
    /// If an <see cref="IKeyVaultService"/> has not previously been initialized in this <see cref="IAzureServiceFactory"/>, 
    /// the factory will try to initialize an instance with default values.
    /// The underlying method for initialization uses <see cref="AddKeyVaultService"/>. 
    /// </summary>
    /// <returns>An instance of <see cref="IKeyVaultService"/>.</returns>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if an <see cref="IKeyVaultService"/> instance cannot be initialized with default values.</para>
    /// </remarks>
    IKeyVaultService KeyVaultService { get; }

    /// <summary>
    /// The <see cref="AddKeyVaultService"/> method initializes and registers an <see cref="IKeyVaultService"/> in this <see cref="IAzureServiceFactory"/>. This requires 
    /// a "KeyVaultUri" variable to be present in environment variables, in local.settings.json, or in the configuration.
    /// </summary>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if a variable for "KeyVaultUri" is not found.</para>
    /// </remarks>
    IAzureServiceFactory AddKeyVaultService();

    /// <summary>
    /// The <see cref="AddKeyVaultService(string)"/> method initializes and registers an <see cref="IKeyVaultService"/> in this <see cref="IAzureServiceFactory"/>.
    /// </summary>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    IAzureServiceFactory AddKeyVaultService(string keyVaultUri);
}