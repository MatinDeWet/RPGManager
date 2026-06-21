using Caching.Configuration;
using Caching.Contracts;
using Caching.UnitTests.TestDoubles;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Caching.UnitTests.Tests;

public class CachingDITests
{
    [Fact]
    public void AddCachingSupport_Default_RegistersCacheServiceAndHybridCache()
    {
        using ServiceProvider provider = CachingTestHost.Build();

        provider.GetService<ICacheService>().ShouldNotBeNull();
        provider.GetService<HybridCache>().ShouldNotBeNull();
    }

    [Fact]
    public void AddCachingSupport_WithConfigureOptions_AppliesProvidedOptions()
    {
        var expiration = TimeSpan.FromHours(2);
        const long maxSize = 4096;

        using ServiceProvider provider = CachingTestHost.Build(options =>
        {
            options.DefaultExpiration = expiration;
            options.MaximumEntrySize = maxSize;
        });

        CacheOptions options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
        options.DefaultExpiration.ShouldBe(expiration);
        options.MaximumEntrySize.ShouldBe(maxSize);
    }

    [Fact]
    public void AddCachingSupport_FromConfiguration_BindsOptionsFromSection()
    {
        Dictionary<string, string?> settings = new()
        {
            [$"{CacheOptions.SectionName}:DefaultExpiration"] = "00:30:00",
            [$"{CacheOptions.SectionName}:MaximumEntrySize"] = "2048",
        };

        using ServiceProvider provider = CachingTestHost.BuildFromConfiguration(settings);

        CacheOptions options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
        options.DefaultExpiration.ShouldBe(TimeSpan.FromMinutes(30));
        options.MaximumEntrySize.ShouldBe(2048);
        provider.GetService<ICacheService>().ShouldNotBeNull();
    }

    [Fact]
    public void AddCachingSupport_FromEmptyConfiguration_UsesDefaultOptions()
    {
        using ServiceProvider provider =
            CachingTestHost.BuildFromConfiguration(new Dictionary<string, string?>());

        CacheOptions options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
        options.DefaultExpiration.ShouldBe(TimeSpan.FromMinutes(5));
    }
}
