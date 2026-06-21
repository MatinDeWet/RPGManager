using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Core.Contracts;
using CQRS.Domain.Contracts;

namespace CQRS.UnitTests.TestDoubles;

public sealed record TestDomainEvent(string Message) : IDomainEvent;

/// <summary>
/// Singleton bridge used by tests to observe that a domain event reached its handler(s).
/// </summary>
public sealed class HandlerSignal
{
    private readonly ConcurrentBag<string> _handledBy = [];

    public TaskCompletionSource<TestDomainEvent> Received { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyCollection<string> HandledBy => _handledBy;

    public void Record(string handlerName, TestDomainEvent domainEvent)
    {
        _handledBy.Add(handlerName);
        Received.TrySetResult(domainEvent);
    }
}

/// <summary>
/// Discovered by <c>AddCQRSSupport</c>'s assembly scan and invoked by the background processor.
/// </summary>
internal sealed class TestDomainEventHandler(HandlerSignal signal) : IDomainEventManager<TestDomainEvent>
{
    public Task Handle(TestDomainEvent request, CancellationToken cancellationToken)
    {
        signal.Record(nameof(TestDomainEventHandler), request);
        return Task.CompletedTask;
    }
}

/// <summary>
/// A second handler for the same event, proving every registered handler is invoked.
/// </summary>
internal sealed class SecondTestDomainEventHandler(HandlerSignal signal) : IDomainEventManager<TestDomainEvent>
{
    public Task Handle(TestDomainEvent request, CancellationToken cancellationToken)
    {
        signal.Record(nameof(SecondTestDomainEventHandler), request);
        return Task.CompletedTask;
    }
}
