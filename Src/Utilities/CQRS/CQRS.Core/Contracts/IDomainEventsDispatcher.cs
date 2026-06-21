using CQRS.Domain.Contracts;

namespace CQRS.Core.Contracts;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
