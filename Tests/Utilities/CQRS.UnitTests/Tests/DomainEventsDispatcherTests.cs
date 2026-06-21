using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Core.Implementation;
using CQRS.Core.Services;
using CQRS.Domain.Contracts;
using CQRS.UnitTests.TestDoubles;
using Shouldly;
using Xunit;

namespace CQRS.UnitTests.Tests;

public class DomainEventsDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_EnqueuesEveryEvent_InOrder()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        BackgroundDomainEventQueue queue = new();
        DomainEventsDispatcher sut = new(queue);

        TestDomainEvent first = new("first");
        TestDomainEvent second = new("second");

        await sut.DispatchAsync([first, second], ct);

        List<IDomainEvent> drained = [];
        await using IAsyncEnumerator<IDomainEvent> enumerator = queue.DequeueAsync(ct).GetAsyncEnumerator(ct);
        while (drained.Count < 2 && await enumerator.MoveNextAsync())
        {
            drained.Add(enumerator.Current);
        }

        drained.ShouldBe([first, second]);
    }

    [Fact]
    public async Task DispatchAsync_EmptyCollection_EnqueuesNothing()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        BackgroundDomainEventQueue queue = new();
        DomainEventsDispatcher sut = new(queue);

        await sut.DispatchAsync([], ct);

        // A subsequently dispatched event must be the very first thing dequeued,
        // proving the empty dispatch enqueued nothing ahead of it.
        TestDomainEvent sentinel = new("sentinel");
        await sut.DispatchAsync([sentinel], ct);

        await using IAsyncEnumerator<IDomainEvent> enumerator = queue.DequeueAsync(ct).GetAsyncEnumerator(ct);
        (await enumerator.MoveNextAsync()).ShouldBeTrue();
        enumerator.Current.ShouldBeSameAs(sentinel);
    }
}
