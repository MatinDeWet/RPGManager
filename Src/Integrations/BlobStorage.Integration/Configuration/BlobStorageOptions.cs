namespace BlobStorage.Configuration;

/// <summary>
/// Connection settings for the S3-compatible (SeaweedFS) blob storage integration.
/// The container/bucket is supplied per call, not configured here.
/// </summary>
public sealed class BlobStorageOptions
{
    /// <summary>
    /// The configuration section name for blob storage options.
    /// </summary>
    public const string SectionName = "BlobStorage";

    /// <summary>
    /// The SeaweedFS S3 endpoint, e.g. "http://localhost:8333".
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// The S3 access key (credential identity).
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// The S3 secret key (credential secret).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// The AWS region label. SeaweedFS ignores it, but the SDK requires a value for signing.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Whether to use path-style addressing instead of virtual-host. Required for SeaweedFS.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    /// Default lifetime for presigned PUT/GET URLs. Default is 5 minutes.
    /// </summary>
    public TimeSpan PresignedUrlExpiry { get; set; } = TimeSpan.FromMinutes(5);
}
