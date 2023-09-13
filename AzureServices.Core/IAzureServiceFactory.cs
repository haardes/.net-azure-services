namespace AzureServices.Core;

public interface IAzureServiceFactory
{
    /// <summary>
    /// The <see cref="KeyVaultService"/> method returns an instance of <see cref="IKeyVaultService"/>. 
    /// If an <see cref="IKeyVaultService"/> has not previously been initialized in this <see cref="IAzureServiceFactory"/>, 
    /// the factory will try to initialize an instance with default values.
    /// The underlying method for initialization uses <see cref="AddKeyVaultService"/>. 
    /// </summary>
    /// <returns>An instance of <see cref="IKeyVaultService"/>.</returns>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if an <see cref="IKeyVaultService"/> instance cannot be initialized with default values.</para>
    /// </remarks>
    IKeyVaultService KeyVaultService();

    /// <summary>
    /// The <see cref="AddKeyVaultService"/> method initializes and registers an <see cref="IKeyVaultService"/> in this <see cref="IAzureServiceFactory"/>. This requires 
    /// a "KeyVaultUri" variable to be present in environment variables, in local.settings.json, or in the configuration.
    /// </summary>
    /// <param name="replace">Determines if an existing <see cref="IKeyVaultService"/> should be replaced if it already exists.</param>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if a variable for "KeyVaultUri" is not found.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if keyVaultUri is <c>null</c> or empty.</exception>
    IAzureServiceFactory AddKeyVaultService(bool replace = false);

    /// <summary>
    /// The <see cref="AddKeyVaultService(string, bool)"/> method initializes and registers an <see cref="IKeyVaultService"/> in this <see cref="IAzureServiceFactory"/>.
    /// </summary>
    /// <param name="keyVaultUri">Determines if an existing <see cref="IKeyVaultService"/> should be replaced if it already exists.</param>
    /// <param name="replace">Determines if an existing <see cref="IKeyVaultService"/> should be replaced if it already exists.</param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if a variable for "KeyVaultUri" is not found.</para>
    /// </remarks>
    /// <returns>This <see cref="IAzureServiceFactory"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if keyVaultUri is <c>null</c> or empty.</exception>
    IAzureServiceFactory AddKeyVaultService(string keyVaultUri, bool replace = false);

    /// <summary>
    /// The <see cref="ThrowIfShouldNotReplace{T}(T, bool)"/> method checks if the <paramref name="instance"/> <typeparamref name="T"/> already exists, 
    /// and throws if it exists and the parameter <paramref name="shouldReplace"/> is false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <param name="shouldReplace">Whether to replace the <paramref name="instance"/> <typeparamref name="T"/> if it exists.</param>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if the <paramref name="instance"/> <typeparamref name="T"/> exists and <paramref name="shouldReplace"/> is false.</para>
    /// </remarks>
    /// <exception cref="Exception">Thrown if the <paramref name="instance"/> <typeparamref name="T"/> exists and <paramref name="shouldReplace"/> is false.</exception>
    void ThrowIfShouldNotReplace<T>(T? instance, bool shouldReplace) where T : class;
}