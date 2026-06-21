using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="User"/>: a user may only see and modify their own record.
/// </summary>
internal sealed class UserLock(CoreContext context) : Lock<User>
{
    public override IQueryable<User> Secured(long userId)
    {
        return from user in context.Set<User>()
               where user.Id == userId
               select user;
    }

    public override Task<bool> HasAccess(User obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return Task.FromResult(obj.Id == userId);
    }
}
