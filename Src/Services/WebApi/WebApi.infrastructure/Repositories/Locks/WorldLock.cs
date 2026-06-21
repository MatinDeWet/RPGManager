using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="World"/>: a user may only see and modify the worlds they own.
/// </summary>
internal sealed class WorldLock(CoreContext context) : Lock<World>
{
    public override IQueryable<World> Secured(long userId)
    {
        return from world in context.Set<World>()
               where world.UserId == userId
               select world;
    }

    public override Task<bool> HasAccess(World obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return Task.FromResult(obj.UserId == userId);
    }
}
