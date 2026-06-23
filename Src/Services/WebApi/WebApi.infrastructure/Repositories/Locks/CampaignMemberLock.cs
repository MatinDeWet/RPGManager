using Microsoft.EntityFrameworkCore;
using Repository.Enums;
using Repository.Lock;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.Locks;

internal sealed class CampaignMemberLock(CoreContext context) : Lock<CampaignMember>
{
    public override IQueryable<CampaignMember> Secured(long userId)
    {
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
