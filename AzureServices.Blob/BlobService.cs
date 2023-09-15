using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureServices.Core;

namespace AzureServices.Blob;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    /// <summary>
    /// Calls <see cref="BlobService(KeyOptions)"/> with <c>new KeyOptions()</c>. For more information on how this constructor works, see <see href="https://github.com/haardes/.net-azure-services"/>.
    /// </summary>
    public BlobService() : this(new KeyOptions()) { }

    /// <summary>
    /// Creates an instance of <see cref="BlobService"/> using the given <paramref name="keyOptions"/>. See <see href="https://github.com/haardes/.net-azure-services"/> for more information on how this constructor works.
    /// </summary>
    /// <param name="keyOptions"></param>
    /// <remarks>
    /// <para>An <see cref="Exception"/> will be thrown if no valid combination of variables are found as either environment variables or as KeyVault secrets.</para>
    /// </remarks>
    /// <exception cref="Exception">Thrown if no valid combination of variables are found as either environment variables or as KeyVault secrets.</exception>
    public BlobService(KeyOptions keyOptions)
    {
        TryGetVariable(keyOptions.StorageAccount(), out string? storageAccount);
        TryGetVariable(keyOptions.StorageKey(), out string? storageKey);
        TryGetVariable(keyOptions.StorageConnectionString(), out string? connectionString);

        if (!string.IsNullOrEmpty(storageAccount) && !string.IsNullOrEmpty(storageKey))
        {
            _blobServiceClient = GetBlobServiceClientFrom(storageAccount, storageKey);
        }
        else if (!string.IsNullOrEmpty(connectionString))
        {
            _blobServiceClient = GetBlobServiceClientFrom(connectionString);
        }
        else
        {
            throw new Exception($"Error when registering BlobServiceClient. Use another constructor or see https://github.com/haardes/.net-azure-services for details on how to use the parameterless BlobService() constructor.");
        }
    }

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public BlobService(string connectionString)
    {
        _blobServiceClient = GetBlobServiceClientFrom(connectionString);
    }

    public BlobService(string storageAccount, string storageKey)
    {
        _blobServiceClient = GetBlobServiceClientFrom(storageAccount, storageKey);
    }

    public BlobServiceClient GetServiceClient()
    {
        return _blobServiceClient;
    }

    public BlobContainerClient GetContainerClient(string containerName)
    {
        return _blobServiceClient.GetBlobContainerClient(containerName);
    }

    public BlobClient GetBlob(string blobPath)
    {
        ThrowIfNotValidBlobName(blobPath);
        var paths = blobPath.Split('/');

        if (paths.Length < 2)
        {
            paths = blobPath.Split('\\');
        }

        var container = paths.First();
        blobPath = string.Join('/', paths.Skip(1));

        return GetBlob(container, blobPath);
    }

    public BlobClient GetBlob(string containerName, string blobName)
    {
        BlobContainerClient container = GetContainerClient(containerName);

        if (!container.Exists())
        {
            throw new DirectoryNotFoundException($"Container {containerName} does not exist.");
        }

        BlobClient blob = container.GetBlobClient(blobName);

        if (!blob.Exists())
        {
            throw new FileNotFoundException($"{blobName} in container {containerName} does not exist.");
        }

        return blob;
    }

    public List<BlobItem> GetBlobs(string containerName, string? prefix = default)
    {
        BlobContainerClient container = GetContainerClient(containerName);

        if (!container.Exists())
        {
            throw new DirectoryNotFoundException($"Container {containerName} does not exist.");
        }

        if (prefix == null)
        {
            return container.GetBlobs().ToList();
        }

        return container.GetBlobs(prefix: prefix).ToList();
    }

    public void UploadBlob(string containerName, string blobName, Stream blobContent, bool overwrite = false)
    {
        BlobContainerClient containerClient = GetContainerClient(containerName);

        if (!containerClient.Exists())
        {
            throw new DirectoryNotFoundException($"Container {containerName} does not exist.");
        }

        try
        {
            blobContent.Position = 0;
            containerClient.UploadBlob(blobName, blobContent);
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode != "BlobAlreadyExists")
            {
                throw ex;
            }

            if (!overwrite)
            {
                throw ex;
            }

            blobContent.Position = 0;
            containerClient.GetBlobClient(blobName).Upload(blobContent, overwrite: true);
        }
        finally
        {
            blobContent.Dispose();
        }
    }

    public void UploadBlob(string containerName, string blobName, string blobContent, bool overwrite = false)
    {
        MemoryStream memory = new();
        StreamWriter writer = new(memory);
        writer.Write(blobContent);
        writer.Flush();

        UploadBlob(containerName, blobName, memory, overwrite);
        writer.Dispose();
    }

    public void UploadBlob(string containerName, string blobName, Stream blobContent, BlobUploadOptions uploadOptions)
    {
        BlobContainerClient containerClient = GetContainerClient(containerName);

        if (!containerClient.Exists())
        {
            throw new DirectoryNotFoundException($"Container {containerName} does not exist.");
        }

        try
        {
            blobContent.Position = 0;
            containerClient.GetBlobClient(blobName).Upload(blobContent, uploadOptions);
        }
        finally
        {
            blobContent.Dispose();
        }
    }

    public void UploadBlob(string containerName, string blobName, string blobContent, BlobUploadOptions uploadOptions)
    {
        MemoryStream memory = new();
        StreamWriter writer = new(memory);
        writer.Write(blobContent);
        writer.Flush();

        UploadBlob(containerName, blobName, memory, uploadOptions);
        writer.Dispose();
    }

    private void ThrowIfNotValidBlobName(string name)
    {
        var container = name.Split('/').FirstOrDefault(string.Empty);

        if (string.IsNullOrEmpty(container))
        {
            container = name.Split('\\').FirstOrDefault(string.Empty);
            if (string.IsNullOrEmpty(container))
            {
                throw new Exception("Blob name does not contain a first \"directory\", and a container name can therefore not be assumed. Check blobName/path.");
            }
        }
    }

    private static void TryGetVariable(string key, out string? value)
    {
        value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrEmpty(value))
        {
            value = new AzureServiceFactory().KeyVaultService().GetSecret(key);
        }
    }

    private static BlobServiceClient GetBlobServiceClientFrom(string storage, string? key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            return new(storage);
        }

        return new(new($"https://{storage}.blob.core.windows.net/"), new StorageSharedKeyCredential(storage, key));
    }
}