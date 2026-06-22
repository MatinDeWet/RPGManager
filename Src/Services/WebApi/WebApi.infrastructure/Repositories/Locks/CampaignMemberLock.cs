using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="CampaignMember"/>: a user may read the roster of any campaign
/// they belong to, but only the campaign's Dungeon Master may modify membership (e.g. kick a player).
/// A player leaving (removing their own membership) is authorised at the handler level through the
/// unsecured repository, since this lock would otherwise block it.
/// </summary>
internal sealed class CampaignMemberLock(CoreContext context) : Lock<CampaignMember>
{
    public override IQueryable<CampaignMember> Secured(long userId)
    {
        // Every member row whose campaign the user is also a member of.
        return from member in context.Set<CampaignMember>()
               where context.Set<CampaignMember>()
                   .Any(membership => membership.CampaignId == member.CampaignId && membership.UserId == userId)
               select member;
    }

    public override async Task<bool> HasAccess(CampaignMember obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return await context.Set<CampaignMember>()
            .AnyAsync(member => member.CampaignId == obj.CampaignId
                && member.UserId == userId
                && member.Role == CampaignRole.DungeonMaster, cancellationToken);
    }
}
