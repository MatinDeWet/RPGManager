using Repository.Contracts;
using Repository.Enums;

namespace Repository.Lock;

/// <summary>
/// Abstract base class for implementing entity-specific security protection rules.
/// Provides a foundation for creating custom security implementations that define
/// both query-level filtering and operation-level access validation for specific entity types.
/// Derived classes implement the concrete security logic for their target entities.
/// </summary>
/// <typeparam name="T">The type of entity that this lock protects.</typeparam>
public abstract class Lock<T> : IProtected<T> where T : class
{
    /// <summary>
    /// Returns a filtered queryable containing only entities the user can access
    /// based on their identity.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A filtered <see cref="IQueryable{T}"/> for the user.</returns>
    public abstract IQueryable<T> Secured(long userId);

    /// <summary>
    /// Validates if the user has permission to perform the operation on the entity.
    /// </summary>
    /// <param name="obj">The entity to check access for.</param>
    /// <param name="userId">The user's ID.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if access is granted; otherwise, <c>false</c>.</returns>
    public abstract Task<bool> HasAccess(T obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken);

    /// <summary>
    /// Determines if this lock applies to the specified entity type.
    /// </summary>
    /// <param name="t">The entity type to check.</param>
    /// <returns><c>true</c> if this lock can handle the specified type; otherwise, <c>false</c>.</returns>
    public virtual bool IsMatch(Type t)
    {
        return typeof(T).IsAssignableFrom(t);
    }
}
