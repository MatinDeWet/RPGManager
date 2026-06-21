using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Repository.Contracts;

namespace Repository.Implementation;

/// <summary>
/// Provides secure query operations by applying entity-specific security filters based on user identity.
/// This class acts as a security gateway for read operations, automatically filtering queryable collections
/// to ensure users can only access data they have permission to view through registered entity protection
/// implementations.
/// </summary>
/// <remarks>
/// The filtering overrides <see cref="QueryRepo{TCtx}.GetQueryable{T}"/>, so security is applied regardless
/// of the static type used to reference the repository (the concrete type, <see cref="QueryRepo{TCtx}"/>,
/// <see cref="IQueryRepo"/> or <see cref="ISecureQueryRepo"/>). There is no unfiltered path.
/// </remarks>
/// <typeparam name="TCtx">The type of Entity Framework DbContext.</typeparam>
public abstract class SecureQueryRepo<TCtx> : QueryRepo<TCtx>, ISecureQueryRepo where TCtx : DbContext
{
    protected readonly IIdentityInfo _info;

    private readonly IEnumerable<IProtected> _protection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureQueryRepo{TCtx}"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework DbContext.</param>
    /// <param name="info">The identity information provider.</param>
    /// <param name="protection">Collection of entity protection implementations.</param>
    protected SecureQueryRepo(TCtx context, IIdentityInfo info, IEnumerable<IProtected> protection) : base(context)
    {
        _info = info;
        _protection = protection;
    }

    /// <summary>
    /// Returns a secure queryable collection with the entity-specific protection filter applied for the
    /// current user. Overrides <see cref="QueryRepo{TCtx}.GetQueryable{T}"/> so the filter cannot be bypassed.
    /// </summary>
    /// <typeparam name="T">The type of entity to query.</typeparam>
    /// <returns>A filtered <see cref="IQueryable{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no protection is registered for <typeparamref name="T"/>, or when the configuration is ambiguous.
    /// </exception>
    public override IQueryable<T> GetQueryable<T>() where T : class
    {
        IProtected<T> entityLock = ProtectionResolver.Resolve<T>(_protection);

        return entityLock.Secured(_info.GetInternalUserId());
    }
}
