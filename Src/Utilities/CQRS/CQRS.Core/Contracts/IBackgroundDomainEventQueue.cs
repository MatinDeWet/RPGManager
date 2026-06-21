using CQRS.Domain.Contracts;

namespace CQRS.Core.Contracts;

public interface IBackgroundDomainEventQueue
{
    ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    IAsyncEnumerable<IDomainEvent> DequeueAsync(CancellationToken cancellationToken = default);
}
