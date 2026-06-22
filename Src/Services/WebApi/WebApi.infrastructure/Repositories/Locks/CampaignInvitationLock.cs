using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

/// <summary>
/// Row-level protection for <see cref="CampaignInvitation"/>: only a campaign's Dungeon Master may
/// read (list) or mutate (create/revoke) its invitations. The invitee-facing accept/decline flows
/// are authorised at the handler level (token-hash lookup + email match) and run through the
/// unsecured repositories, since the invitee is not yet a member and this lock would otherwise
/// block them.
/// </summary>
internal sealed class CampaignInvitationLock(CoreContext context) : Lock<CampaignInvitation>
{
    public override IQueryable<CampaignInvitation> Secured(long userId)
    {
        return from member in context.Set<CampaignMember>()
               join invitation in context.Set<CampaignInvitation>()
                   on member.CampaignId equals invitation.CampaignId
               where member.UserId == userId && member.Role == CampaignRole.DungeonMaster
               select invitation;
    }

    public override async Task<bool> HasAccess(CampaignInvitation obj, long userId, RepositoryOperationEnum operation, CancellationToken cancellationToken)
    {
        return await context.Set<CampaignMember>()
            .AnyAsync(member => member.CampaignId == obj.CampaignId
                && member.UserId == userId
                && member.Role == CampaignRole.DungeonMaster, cancellationToken);
    }
}
