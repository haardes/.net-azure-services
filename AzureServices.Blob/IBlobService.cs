using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureServices.Blob;

public interface IBlobService
{
    BlobServiceClient GetServiceClient();

    BlobContainerClient GetContainerClient(string containerName);

    BlobClient GetBlob(string blobName);

    /// <summary>
    /// The <see cref="GetBlob(string, string)"/> operation returns a blob in the specified <paramref name="container"/> with the given <paramref name="blobName"/>, if it exists.
    /// </summary>
    /// <param name="containerName">Name of container.</param>
    /// <param name="blobName">Name of file, including its path if any.</param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="FileNotFoundException"/> will be thrown if the blob does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the blob does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if a failure occurs.</exception>
    /// <returns>A <see cref="BlobClient"/> instance.</returns>
    BlobClient GetBlob(string container, string blobName);

    /// <summary>
    /// The <see cref="GetBlobs"/> operation returns a list of blobs in the specified <paramref name="container"/> with an optional <paramref name="prefix"/>.
    /// </summary>
    /// <param name="containerName">Name of container.</param>
    /// <param name="prefix">Prefix for blobs.</param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if a failure occurs.</exception>
    /// <returns>A <see cref="List{BlobItem}"/> of <see cref="BlobItem"/></returns>
    List<BlobItem> GetBlobs(string container, string? prefix = default);

    /// <summary>
    /// The <see cref="UploadBlob(string, string, Stream, bool)"/> operation uploads a stream to the specified container.
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="blobName"></param>
    /// <param name="blobContent"></param>
    /// <param name="overwrite"></param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if the blob already exists and overwrite is NOT set to true, or if a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if the blob already exists and overwrite is NOT set to true, or if a failure occurs.</exception>
    void UploadBlob(string containerName, string blobName, Stream blobContent, bool overwrite = false);

    /// <summary>
    /// The <see cref="UploadBlob(string, string, string, bool)"/> operation uploads a string to the specified container.
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="blobName"></param>
    /// <param name="blobContent"></param>
    /// <param name="overwrite"></param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if the blob already exists and overwrite is NOT set to true, or if a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if the blob already exists and overwrite is NOT set to true, or if a failure occurs.</exception>
    void UploadBlob(string containerName, string blobName, string blobContent, bool overwrite = false);
}