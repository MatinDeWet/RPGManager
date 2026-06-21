using Repository.Enums;

namespace Repository.Contracts;

/// <summary>
/// Base interface for entity protection implementations that provides runtime type and identity matching capability.
/// Used by the SecureRepository system to dynamically identify which protection rules apply to specific entity types and identity contexts.
/// </summary>
public interface IProtected
{
    /// <summary>
    /// Determines if this protection implementation can handle the specified entity type.
    /// This enables the SecureRepository system to dynamically route entities to their appropriate security implementations
    /// based on the entity type.
    /// </summary>
    /// <param name="t">The entity type to check for compatibility.</param>
    /// <returns><c>true</c> if this protection can handle the specified entity type; otherwise, <c>false</c>.</returns>
    bool IsMatch(Type t);
}

/// <summary>
/// Defines comprehensive entity-specific security operations including query filtering and access validation.
/// Implementations of this interface provide custom security logic for specific entity types, enabling
/// fine-grained access control based on user identity and operation context.
/// </summary>
/// <typeparam name="T">The type of entity that this protection implementation secures.</typeparam>
public interface IProtected<T> : IProtected where T : class
{
    /// <summary>
    /// Returns a pre-filtered queryable collection containing only entities the user is authorized to access.
    /// This method enables transparent security filtering at the query level, ensuring users can only
    /// retrieve data they have permission to see without requiring explicit permission checks in business logic.
    /// </summary>
    /// <param name="userId">The user's ID for security filtering.</param>
    /// <returns>A filtered <see cref="IQueryable{T}"/> with user-specific security applied.</returns>
    IQueryable<T> Secured(long userId);

    /// <summary>
    /// Validates if the user has permission to perform the specified operation on the entity.
    /// This method is called before any data modification operations (insert, update, delete) to ensure
    /// the user has the appropriate permissions for the requested action on the specific entity instance.
    /// </summary>
    /// <param name="obj">The entity to validate access for.</param>
    /// <param name="userId">The user's ID.</param>
    /// <param name="operation">The type of operation being performed (Insert, Update, Delete, Read).</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns><c>true</c> if access is granted; otherwise, <c>false</c>.</returns>
    Task<bool> HasAccess(T obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken);
}
