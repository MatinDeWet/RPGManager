using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

internal sealed class SessionLock(CoreContext context) : Lock<Session>
{
    public override IQueryable<Session> Secured(long userId)
    {
        return from member in context.Set<CampaignMember>()
               join session in context.Set<Session>()
                   on member.CampaignId equals session.CampaignId
               where member.UserId == userId
               select session;
    }

    public override async Task<bool> HasAccess(Session obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        IQueryable<CampaignMember> membership = context.Set<CampaignMember>()
            .Where(member => member.CampaignId == obj.CampaignId && member.UserId == userId);

        if (operation is RepositoryOperationEnum.Insert or RepositoryOperationEnum.Update or RepositoryOperationEnum.Delete)
        {
            membership = membership.Where(member => member.Role == CampaignRole.DungeonMaster);
        }

        return await membership.AnyAsync(cancellationToken);
    }
}
