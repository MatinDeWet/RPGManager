namespace Caching.Contracts;

/// <summary>
/// Provides a unified interface for caching operations using HybridCache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache or creates it if it doesn't exist.
    /// </summary>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">A factory function to create the value if it doesn't exist in cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    ValueTask<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from the cache or creates it if it doesn't exist, with custom expiration options.
    /// </summary>
    /// <typeparam name="TValue">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">A factory function to create the value if it doesn't exist in cache.</param>
    /// <param name="expiration">The expiration time for this specific cache entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    ValueTask<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional expiration time. If null, uses default expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple values from the cache by their keys.
    /// </summary>
    /// <param name="keys">The cache keys to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
}
