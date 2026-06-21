namespace Domain.Contracts;

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; }
}

public interface IEntity
{
    DateTimeOffset DateCreated { get; }
}
