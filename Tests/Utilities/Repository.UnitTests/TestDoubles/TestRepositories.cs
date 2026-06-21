using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Repository.Contracts;
using Repository.Enums;
using Repository.Implementation;
using Repository.Lock;

namespace Repository.UnitTests.TestDoubles;

/// <summary>
/// Concrete <see cref="QueryRepo{TCtx}"/> over a plain <see cref="DbContext"/> so the abstract base can be exercised.
/// </summary>
internal sealed class TestQueryRepo(DbContext context) : QueryRepo<DbContext>(context);

/// <summary>
/// Concrete <see cref="CommandRepo{TCtx}"/> over a plain <see cref="DbContext"/>.
/// </summary>
internal sealed class TestCommandRepo(DbContext context) : CommandRepo<DbContext>(context);

/// <summary>
/// Concrete <see cref="SecureQueryRepo{TCtx}"/> over a plain <see cref="DbContext"/>.
/// </summary>
internal sealed class TestSecureQueryRepo(DbContext context, IIdentityInfo info, IEnumerable<IProtected> protection)
    : SecureQueryRepo<DbContext>(context, info, protection);

/// <summary>
/// Concrete <see cref="SecureCommandRepo{TCtx}"/> over a plain <see cref="DbContext"/>.
/// </summary>
internal sealed class TestSecureCommandRepo(DbContext context, IIdentityInfo info, IEnumerable<IProtected> protection)
    : SecureCommandRepo<DbContext>(context, info, protection);

/// <summary>
/// Concrete <see cref="Lock{T}"/> used to verify the default <see cref="Lock{T}.IsMatch"/> behaviour.
/// </summary>
internal sealed class TestLock : Lock<TestEntity>
{
    public override IQueryable<TestEntity> Secured(long userId)
    {
        return Array.Empty<TestEntity>().AsQueryable();
    }

    public override Task<bool> HasAccess(TestEntity obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
