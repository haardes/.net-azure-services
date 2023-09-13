using AzureServices.Core;

namespace AzureServices.Blob;

public static class AzureServiceFactoryExtensions
{
    private static IBlobService? _blobService;

    public static IBlobService BlobService(this IAzureServiceFactory azureServiceFactory)
    {
        return _blobService;
    }

    public static void AddBlobService(this IAzureServiceFactory azureServiceFactory)
    {

    }
}