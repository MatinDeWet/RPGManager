using Caching.Configuration;
using Caching.Contracts;
using Caching.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Caching;

/// <summary>
/// Dependency injection extensions for caching services.
/// </summary>
public static class CachingDI
{
    /// <summary>
    /// Adds caching services to the service collection with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachingSupport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        // Add HybridCache with configuration
        services.AddHybridCache(options =>
        {
            CacheOptions cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>()
                               ?? new CacheOptions();

            options.DefaultEntryOptions = new()
            {
                Expiration = cacheOptions.DefaultExpiration,
                LocalCacheExpiration = cacheOptions.DefaultExpiration
            };

            options.MaximumPayloadBytes = cacheOptions.MaximumEntrySize;
        });

        // Register cache service
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// Adds caching services to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachingSupport(
        this IServiceCollection services,
        Action<CacheOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        var cacheOptions = new CacheOptions();
        configureOptions(cacheOptions);

        // Add HybridCache with configuration
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new()
            {
                Expiration = cacheOptions.DefaultExpiration,
                LocalCacheExpiration = cacheOptions.DefaultExpiration
            };

            options.MaximumPayloadBytes = cacheOptions.MaximumEntrySize;
        });

        // Register cache service
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// Adds caching services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCachingSupport(this IServiceCollection services)
    {
        // Use default options
        services.Configure<CacheOptions>(_ => { });

        // Add HybridCache with default settings
        services.AddHybridCache();

        // Register cache service
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
