using CQRS.Domain.Contracts;
using CQRS.Core.Contracts;

namespace CQRS.Core.Implementation;

internal sealed class DomainEventsDispatcher(IBackgroundDomainEventQueue eventQueue) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await eventQueue.EnqueueAsync(domainEvent, cancellationToken);
        }
    }
}
