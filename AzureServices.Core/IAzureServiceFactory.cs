namespace AzureServices.Core;

public interface IAzureServiceFactory
{
    AzureServiceFactory AddService<T>(T service) where T : IAzureService;
}