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
        return from campaign in context.Set<Campaign>()
               where context.Set<CampaignMember>()
                   .Any(member => member.CampaignId == campaign.Id && member.UserId == userId)
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

        // Mutating operations require the Dungeon Master role; reads only require membership.
        bool requiresDungeonMaster = operation is RepositoryOperationEnum.Update or RepositoryOperationEnum.Delete;

        return await context.Set<CampaignMember>()
            .AnyAsync(
                member => member.CampaignId == obj.Id
                    && member.UserId == userId
                    && (!requiresDungeonMaster || member.Role == CampaignRole.DungeonMaster),
                cancellationToken);
    }
}
