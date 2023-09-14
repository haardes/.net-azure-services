using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureServices.Blob;

public interface IBlobService
{
    /// <summary>
    /// The <see cref="GetServiceClient"/> method returns the underlying <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <returns>The underlying <see cref="BlobServiceClient"/> instance.</returns>
    BlobServiceClient GetServiceClient();

    /// <summary>
    /// The <see cref="GetContainerClient(string)"/> method returns a <see cref="BlobContainerClient"/> for the given <paramref name="container"/>.
    /// </summary>
    /// <param name="container">Name of container.</param>
    /// <returns>The <see cref="BlobContainerClient"/> for the given <paramref name="container"/>.</returns>
    BlobContainerClient GetContainerClient(string container);

    /// <summary>
    /// The <see cref="GetBlob(string)"/> method returns a <see cref="BlobClient"/> for the given <paramref name="blobPath"/>.
    /// </summary>
    /// <param name="blobPath">Path to blob.</param>
    /// <returns>The <see cref="BlobClient"/> for the given path.</returns>
    BlobClient GetBlob(string blobPath);

    /// <summary>
    /// The <see cref="GetBlob(string, string)"/> method returns a blob in the specified <paramref name="container"/> with the given <paramref name="blobName"/>, if it exists.
    /// </summary>
    /// <param name="container">Name of container.</param>
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
    /// The <see cref="GetBlobs"/> method returns a list of blobs in the specified <paramref name="container"/> with an optional <paramref name="prefix"/>.
    /// </summary>
    /// <param name="container">Name of container.</param>
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
    /// The <see cref="UploadBlob(string, string, Stream, bool)"/> method uploads a stream to the specified container.
    /// </summary>
    /// <param name="container">Name of container.</param>
    /// <param name="blobName">Name of file, including its path if any.</param>
    /// <param name="blobContent">Content to upload.</param>
    /// <param name="overwrite">Determines if the blob should be overwritten if it already exists.</param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if the blob already exists and overwrite is not set, or a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if the blob already exists and overwrite is not set, or a failure occurs.</exception>
    void UploadBlob(string container, string blobName, Stream blobContent, bool overwrite = false);

    /// <summary>
    /// The <see cref="UploadBlob(string, string, string, bool)"/> method uploads a string to the specified container.
    /// </summary>
    /// <param name="container">Name of container.</param>
    /// <param name="blobName">Name of file, including its path if any.</param>
    /// <param name="blobContent">Content to upload.</param>
    /// <param name="overwrite">Determines if the blob should be overwritten if it already exists.</param>
    /// <remarks>
    /// <para>A <see cref="DirectoryNotFoundException"/> will be thrown if the container does not exist.</para>
    /// <para>A <see cref="RequestFailedException"/> will be thrown if the blob already exists and overwrite is not set, or a failure occurs.</para>
    /// </remarks>
    /// <exception cref="DirectoryNotFoundException">Thrown if the container does not exist.</exception>
    /// <exception cref="RequestFailedException">Thrown if the blob already exists and overwrite is not set, or a failure occurs.</exception>
    void UploadBlob(string container, string blobName, string blobContent, bool overwrite = false);
}