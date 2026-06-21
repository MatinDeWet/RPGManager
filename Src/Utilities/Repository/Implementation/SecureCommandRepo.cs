using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Repository.Contracts;
using Repository.Enums;

namespace Repository.Implementation;

/// <summary>
/// Provides secure command operations by validating user permissions before executing data modifications.
/// This class acts as a security gateway for write operations (insert, update, delete), ensuring all data
/// modification attempts are authorized through entity-specific protection implementations before being
/// staged to the database context.
/// </summary>
/// <remarks>
/// The checks override the <see cref="CommandRepo{TCtx}"/> modification methods, so validation is applied
/// regardless of the static type used to reference the repository (the concrete type,
/// <see cref="CommandRepo{TCtx}"/>, <see cref="ICommandRepo"/> or <see cref="ISecureCommandRepo"/>). The
/// inherited persist-immediately overloads route through these overrides, so there is no unvalidated path.
/// </remarks>
/// <typeparam name="TCtx">The type of Entity Framework DbContext.</typeparam>
public abstract class SecureCommandRepo<TCtx> : CommandRepo<TCtx>, ISecureCommandRepo where TCtx : DbContext
{
    protected readonly IIdentityInfo _info;

    private readonly IEnumerable<IProtected> _protection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureCommandRepo{TCtx}"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework DbContext.</param>
    /// <param name="info">The identity information provider.</param>
    /// <param name="protection">Collection of entity protection implementations.</param>
    protected SecureCommandRepo(TCtx context, IIdentityInfo info, IEnumerable<IProtected> protection) : base(context)
    {
        _info = info;
        _protection = protection;
    }

    /// <summary>
    /// Validates permissions and stages the entity for insertion. Overrides
    /// <see cref="CommandRepo{TCtx}.InsertAsync{T}(T, CancellationToken)"/> so the check cannot be bypassed.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user does not have permission to insert the entity.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no protection is registered for the entity type.</exception>
    public override async Task InsertAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        await EnsureAccess(obj, RepositoryOperationEnum.Insert, cancellationToken);

        _context.Add(obj);
    }

    /// <summary>
    /// Validates permissions and stages the entity for update. Overrides
    /// <see cref="CommandRepo{TCtx}.UpdateAsync{T}(T, CancellationToken)"/> so the check cannot be bypassed.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user does not have permission to update the entity.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no protection is registered for the entity type.</exception>
    public override async Task UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        await EnsureAccess(obj, RepositoryOperationEnum.Update, cancellationToken);

        _context.Update(obj);
    }

    /// <summary>
    /// Validates permissions and stages the entity for deletion. Overrides
    /// <see cref="CommandRepo{TCtx}.DeleteAsync{T}(T, CancellationToken)"/> so the check cannot be bypassed.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user does not have permission to delete the entity.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no protection is registered for the entity type.</exception>
    public override async Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        await EnsureAccess(obj, RepositoryOperationEnum.Delete, cancellationToken);

        _context.Remove(obj);
    }

    /// <summary>
    /// Validates user access for the specified operation on the entity and throws when it is denied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to check.</param>
    /// <param name="operation">The operation type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when access is denied.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no protection is registered for the entity type.</exception>
    private async Task EnsureAccess<T>(T obj, RepositoryOperationEnum operation, CancellationToken cancellationToken) where T : class
    {
        IProtected<T> entityLock = ProtectionResolver.Resolve<T>(_protection);

        bool hasAccess = await entityLock.HasAccess(obj, _info.GetInternalUserId(), operation, cancellationToken);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException();
        }
    }
}
