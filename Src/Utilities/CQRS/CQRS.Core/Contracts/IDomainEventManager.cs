using CQRS.Domain.Contracts;

namespace CQRS.Core.Contracts;

public interface IDomainEventManager<in T> where T : IDomainEvent
{
    Task Handle(T request, CancellationToken cancellationToken);
}
