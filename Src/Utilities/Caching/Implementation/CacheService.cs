using Caching.Contracts;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Caching.Implementation;

/// <summary>
/// Implementation of ICacheService using Microsoft.Extensions.Caching.Hybrid.
/// </summary>
internal sealed class CacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly Configuration.CacheOptions _options;

    public CacheService(HybridCache hybridCache, IOptions<Configuration.CacheOptions> options)
    {
        _hybridCache = hybridCache;
        _options = options.Value;
    }

    public async ValueTask<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        CancellationToken cancellationToken = default)
    {
        return await _hybridCache.GetOrCreateAsync(
            key,
            factory,
            options: new HybridCacheEntryOptions
            {
                Expiration = _options.DefaultExpiration,
                LocalCacheExpiration = _options.DefaultExpiration
            },
            cancellationToken: cancellationToken);
    }

    public async ValueTask<TValue> GetOrCreateAsync<TValue>(
        string key,
        Func<CancellationToken, ValueTask<TValue>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        return await _hybridCache.GetOrCreateAsync(
            key,
            factory,
            options: new HybridCacheEntryOptions
            {
                Expiration = expiration,
                LocalCacheExpiration = expiration
            },
            cancellationToken: cancellationToken);
    }

    public async ValueTask SetAsync<TValue>(
        string key,
        TValue value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan cacheExpiration = expiration ?? _options.DefaultExpiration;

        await _hybridCache.SetAsync(
            key,
            value,
            options: new HybridCacheEntryOptions
            {
                Expiration = cacheExpiration,
                LocalCacheExpiration = cacheExpiration
            },
            cancellationToken: cancellationToken);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveAsync(key, cancellationToken);
    }

    public async ValueTask RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveAsync([.. keys], cancellationToken);
    }
}
