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
    /// For more information on how this constructor works, see <see href="https://github.com/haardes/.net-azure-services"/>.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public BlobService()
    {
        TryGetVariable("StorageAccount", out string? storageAccount);
        TryGetVariable("StorageAccount", out string? storageKey);
        TryGetVariable("StorageAccount", out string? connectionString);

        if (!string.IsNullOrEmpty(storageAccount) && !string.IsNullOrEmpty(storageKey))
        {
            _blobServiceClient = GetBlobServiceClientFrom(storageAccount, storageKey);
        } else if (!string.IsNullOrEmpty(connectionString))
        {
            _blobServiceClient = GetBlobServiceClientFrom(connectionString);
        } else
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

    public BlobClient GetBlob(string blobName)
    {
        ThrowIfNotValidBlobName();
        throw new NotImplementedException();
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

    private void ThrowIfNotValidBlobName()
    {
        throw new NotImplementedException();
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