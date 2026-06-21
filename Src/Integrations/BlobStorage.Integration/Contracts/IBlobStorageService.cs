using BlobStorage.Contracts.Models;

namespace BlobStorage.Contracts;

/// <summary>
/// S3-compatible blob storage operations against SeaweedFS. The target container (bucket)
/// is supplied by the caller on every operation.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Deletes a blob by key. Succeeds (no-op) if the blob does not exist.
    /// </summary>
    Task DeleteAsync(string container, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a blob directly from a stream, creating the container if it does not exist.
    /// The caller retains ownership of <paramref name="content"/> and is responsible for
    /// disposing it.
    /// </summary>
    Task UploadAsync(
        string container,
        string key,
        Stream content,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the container exists (creating it if needed), then reserves a key and returns a
    /// short-lived presigned PUT URL so a client can upload directly to SeaweedFS. The blob
    /// materializes when the client uploads to the URL.
    /// </summary>
    Task<BlobUploadTicket> CreateUploadUrlAsync(
        string container,
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a blob's metadata (content type and length) without downloading its payload.
    /// Returns <c>null</c> if the blob does not exist.
    /// </summary>
    Task<BlobMetadata?> GetMetadataAsync(string container, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a blob directly. The returned <see cref="BlobContent.Content"/> stream is
    /// owned by the caller and must be disposed.
    /// </summary>
    Task<BlobContent> DownloadAsync(string container, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a short-lived presigned GET URL for reading/downloading a blob.
    /// </summary>
    BlobDownloadTicket CreateDownloadUrl(string container, string key, TimeSpan? expiry = null);
}
