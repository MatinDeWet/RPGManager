using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="Campaign"/>: a user may read a campaign they are a member of
/// (Player or Dungeon Master), but may only modify it as a Dungeon Master. Creation is authorised at
/// the handler level (the caller must own the parent world) and enrols the creator as the first
/// Dungeon Master.
/// </summary>
internal sealed class CampaignLock(CoreContext context) : Lock<Campaign>
{
    public override IQueryable<Campaign> Secured(long userId)
    {
        // Driven from the membership side so the planner uses IX_CampaignMember_UserId to resolve the
        // (typically small) set of campaigns the user belongs to. The unique (CampaignId, UserId) key
        // guarantees one membership row per campaign, so the join yields no duplicate campaigns.
        return from member in context.Set<CampaignMember>()
               join campaign in context.Set<Campaign>()
                   on member.CampaignId equals campaign.Id
               where member.UserId == userId
               select campaign;
    }

    public override async Task<bool> HasAccess(Campaign obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        // The campaign has no members yet at creation time, so authorise the insert here; the parent
        // world ownership check lives in the create handler.
        if (operation == RepositoryOperationEnum.Insert)
        {
            return true;
        }

        // Resolved by the composite (CampaignId, UserId) primary key.
        IQueryable<CampaignMember> membership = context.Set<CampaignMember>()
            .Where(member => member.CampaignId == obj.Id && member.UserId == userId);

        // Mutating operations require the Dungeon Master role; reads only require membership.
        if (operation is RepositoryOperationEnum.Update or RepositoryOperationEnum.Delete)
        {
            membership = membership.Where(member => member.Role == CampaignRole.DungeonMaster);
        }

        return await membership.AnyAsync(cancellationToken);
    }
}
