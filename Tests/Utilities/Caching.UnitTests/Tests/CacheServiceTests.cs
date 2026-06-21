using Caching.Contracts;
using Caching.UnitTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Caching.UnitTests.Tests;

public class CacheServiceTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetOrCreateAsync_InvokesFactoryOnce_WhenKeyRequestedTwice()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);
        int factoryCalls = 0;

        int first = await sut.GetOrCreateAsync("key", _ =>
        {
            factoryCalls++;
            return ValueTask.FromResult(42);
        }, Token);

        int second = await sut.GetOrCreateAsync("key", _ =>
        {
            factoryCalls++;
            return ValueTask.FromResult(99);
        }, Token);

        first.ShouldBe(42);
        second.ShouldBe(42);
        factoryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExpiration_ReturnsFactoryValue()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);

        string result = await sut.GetOrCreateAsync(
            "key",
            _ => ValueTask.FromResult("value"),
            TimeSpan.FromMinutes(1),
            Token);

        result.ShouldBe("value");
    }

    [Fact]
    public async Task SetAsync_ThenGetOrCreateAsync_ReturnsStoredValue_WithoutInvokingFactory()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);
        bool factoryInvoked = false;

        await sut.SetAsync("key", "stored", cancellationToken: Token);

        string result = await sut.GetOrCreateAsync("key", _ =>
        {
            factoryInvoked = true;
            return ValueTask.FromResult("from-factory");
        }, Token);

        result.ShouldBe("stored");
        factoryInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task SetAsync_WithExplicitExpiration_StoresValue()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);

        await sut.SetAsync("key", 7, TimeSpan.FromMinutes(10), Token);

        int result = await sut.GetOrCreateAsync("key", _ => ValueTask.FromResult(-1), Token);
        result.ShouldBe(7);
    }

    [Fact]
    public async Task RemoveAsync_EvictsEntry_SoFactoryRunsAgain()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);

        await sut.SetAsync("key", "original", cancellationToken: Token);
        await sut.RemoveAsync("key", Token);

        string result = await sut.GetOrCreateAsync(
            "key",
            _ => ValueTask.FromResult("recreated"),
            Token);

        result.ShouldBe("recreated");
    }

    [Fact]
    public async Task RemoveAsync_WithMultipleKeys_EvictsAllOfThem()
    {
        using ServiceProvider provider = CachingTestHost.Build();
        ICacheService sut = CachingTestHost.Resolve(provider);

        await sut.SetAsync("a", "a-value", cancellationToken: Token);
        await sut.SetAsync("b", "b-value", cancellationToken: Token);

        await sut.RemoveAsync(["a", "b"], Token);

        string a = await sut.GetOrCreateAsync("a", _ => ValueTask.FromResult("a-new"), Token);
        string b = await sut.GetOrCreateAsync("b", _ => ValueTask.FromResult("b-new"), Token);

        a.ShouldBe("a-new");
        b.ShouldBe("b-new");
    }
}
