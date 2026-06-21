using System.Diagnostics.CodeAnalysis;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using BlobStorage.Configuration;
using BlobStorage.Contracts;
using BlobStorage.Contracts.Models;
using BlobStorage.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobStorage.Implementation;

/// <summary>
/// <see cref="IBlobStorageService"/> backed by an S3-compatible SeaweedFS endpoint.
/// </summary>
internal sealed class SeaweedBlobStorageService : IBlobStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly BlobStorageOptions _options;
    private readonly ILogger<SeaweedBlobStorageService> _logger;

    public SeaweedBlobStorageService(
        IAmazonS3 s3,
        IOptions<BlobStorageOptions> options,
        ILogger<SeaweedBlobStorageService> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    public async Task DeleteAsync(string container, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);

        await _s3.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = container, Key = key },
            cancellationToken);
    }

    public async Task UploadAsync(
        string container,
        string key,
        Stream content,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(content);

        await EnsureContainerExistsAsync(container, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = container,
            Key = key,
            InputStream = content,
            AutoCloseStream = false,
            ContentType = contentType,
        };

        await _s3.PutObjectAsync(request, cancellationToken);
    }

    public async Task<BlobUploadTicket> CreateUploadUrlAsync(
        string container,
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);

        await EnsureContainerExistsAsync(container, cancellationToken);

        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(expiry ?? _options.PresignedUrlExpiry);

        string url = await _s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = container,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
        });

        return new BlobUploadTicket(key, new Uri(url), expiresAt);
    }

    public async Task<BlobMetadata?> GetMetadataAsync(string container, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);

        try
        {
            GetObjectMetadataResponse response = await _s3.GetObjectMetadataAsync(
                new GetObjectMetadataRequest { BucketName = container, Key = key },
                cancellationToken);

            return new BlobMetadata(key, response.Headers.ContentType, response.ContentLength);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Stream ownership is transferred to the caller via BlobContent.")]
    public async Task<BlobContent> DownloadAsync(string container, string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);

        GetObjectResponse response = await _s3.GetObjectAsync(
            new GetObjectRequest { BucketName = container, Key = key },
            cancellationToken);

        return new BlobContent(
            key,
            response.ResponseStream,
            response.Headers.ContentType,
            response.ContentLength);
    }

    public BlobDownloadTicket CreateDownloadUrl(string container, string key, TimeSpan? expiry = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(key);

        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.Add(expiry ?? _options.PresignedUrlExpiry);

        string url = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = container,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = expiresAt.UtcDateTime,
        });

        return new BlobDownloadTicket(key, new Uri(url), expiresAt);
    }

    private async Task EnsureContainerExistsAsync(string container, CancellationToken cancellationToken)
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_s3, container))
        {
            return;
        }

        try
        {
            await _s3.PutBucketAsync(new PutBucketRequest { BucketName = container }, cancellationToken);
            _logger.ContainerCreated(container);
        }
        catch (AmazonS3Exception ex) when (ex is BucketAlreadyOwnedByYouException or BucketAlreadyExistsException)
        {
            _logger.ContainerCreatedConcurrently(container);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.ContainerCreateFailed(ex, container);
            throw;
        }
    }
}
