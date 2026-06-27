using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="Session"/>: any campaign member may read a campaign's
/// sessions, but only a Dungeon Master may create, update or delete them.
/// </summary>
internal sealed class SessionLock(CoreContext context) : Lock<Session>
{
    public override IQueryable<Session> Secured(long userId)
    {
        // Driven from the membership side so the planner uses IX_CampaignMember_UserId. The unique
        // (CampaignId, UserId) key yields one membership row per campaign, so no session is duplicated.
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

        // Reads only require membership; all mutating operations require the Dungeon Master role.
        if (operation is RepositoryOperationEnum.Insert or RepositoryOperationEnum.Update or RepositoryOperationEnum.Delete)
        {
            membership = membership.Where(member => member.Role == CampaignRole.DungeonMaster);
        }

        return await membership.AnyAsync(cancellationToken);
    }
}
