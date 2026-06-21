namespace Caching.Configuration;

/// <summary>
/// Configuration options for the caching service.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// The configuration section name for cache options.
    /// </summary>
    public const string SectionName = "Cache";

    /// <summary>
    /// Default expiration time for cache entries. Default is 5 minutes.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum entry size in bytes. Default is 1 MB.
    /// </summary>
    public long MaximumEntrySize { get; set; } = 1024 * 1024;

    /// <summary>
    /// Whether to enable distributed caching. Default is false (memory cache only).
    /// </summary>
    public bool EnableDistributedCache { get; set; }

    /// <summary>
    /// Connection string for distributed cache (e.g., Redis).
    /// </summary>
    public string? DistributedCacheConnectionString { get; set; }
}
