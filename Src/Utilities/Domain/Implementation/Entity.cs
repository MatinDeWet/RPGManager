using Domain.Contracts;

namespace Domain.Implementation;

public abstract class Entity<T> : Entity, IEntity<T>
{
    public T Id { get; protected set; } = default!;
}

public abstract class Entity : IEntity
{
    public DateTimeOffset DateCreated { get; protected set; } = DateTimeOffset.UtcNow;
}
