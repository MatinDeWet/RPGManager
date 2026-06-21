using Caching;
using Caching.Configuration;
using Caching.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Caching.UnitTests.TestDoubles;

/// <summary>
/// Builds a real service provider wired with the caching utility (backed by the
/// in-memory HybridCache), so tests exercise the genuine DI wiring and cache behavior.
/// </summary>
internal static class CachingTestHost
{
    public static ServiceProvider Build(Action<CacheOptions>? configureOptions = null)
    {
        var services = new ServiceCollection();

        if (configureOptions is null)
        {
            services.AddCachingSupport();
        }
        else
        {
            services.AddCachingSupport(configureOptions);
        }

        return services.BuildServiceProvider();
    }

    public static ServiceProvider BuildFromConfiguration(IDictionary<string, string?> settings)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddCachingSupport(configuration);

        return services.BuildServiceProvider();
    }

    public static ICacheService Resolve(ServiceProvider provider) =>
        provider.GetRequiredService<ICacheService>();
}
