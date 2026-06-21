using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Core.Services;
using CQRS.Domain.Contracts;
using CQRS.UnitTests.TestDoubles;
using Shouldly;
using Xunit;

namespace CQRS.UnitTests.Tests;

public class BackgroundDomainEventQueueTests
{
    [Fact]
    public async Task EnqueueAsync_ThenDequeue_YieldsSameEvent()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        BackgroundDomainEventQueue sut = new();
        TestDomainEvent published = new("payload");

        await sut.EnqueueAsync(published, ct);

        await using IAsyncEnumerator<IDomainEvent> enumerator = sut.DequeueAsync(ct).GetAsyncEnumerator(ct);
        (await enumerator.MoveNextAsync()).ShouldBeTrue();
        enumerator.Current.ShouldBeSameAs(published);
    }

    [Fact]
    public async Task EnqueueAsync_PreservesFifoOrdering()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        BackgroundDomainEventQueue sut = new();
        TestDomainEvent first = new("first");
        TestDomainEvent second = new("second");

        await sut.EnqueueAsync(first, ct);
        await sut.EnqueueAsync(second, ct);

        await using IAsyncEnumerator<IDomainEvent> enumerator = sut.DequeueAsync(ct).GetAsyncEnumerator(ct);

        (await enumerator.MoveNextAsync()).ShouldBeTrue();
        enumerator.Current.ShouldBeSameAs(first);
        (await enumerator.MoveNextAsync()).ShouldBeTrue();
        enumerator.Current.ShouldBeSameAs(second);
    }

    [Fact]
    public async Task EnqueueAsync_Null_Throws()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        BackgroundDomainEventQueue sut = new();

        await Should.ThrowAsync<ArgumentNullException>(async () => await sut.EnqueueAsync(null!, ct));
    }
}
