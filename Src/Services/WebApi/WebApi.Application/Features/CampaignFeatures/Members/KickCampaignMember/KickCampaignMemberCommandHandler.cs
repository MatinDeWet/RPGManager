using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Members.KickCampaignMember;

internal sealed class KickCampaignMemberCommandHandler(
    ICampaignMemberSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<KickCampaignMemberCommand>
{
    public async Task<Result> Handle(KickCampaignMemberCommand request, CancellationToken cancellationToken)
    {
        // CampaignMembers is row-level filtered to campaigns the caller is a member of.
        CampaignMember? target = await queryRepo.CampaignMembers
            .FirstOrDefaultAsync(x => x.CampaignId == request.CampaignId && x.UserId == request.UserId, cancellationToken);

        if (target is null)
        {
            return Result.NotFound($"User '{request.UserId}' is not a member of campaign '{request.CampaignId}'.");
        }

        if (target.Role == CampaignRole.DungeonMaster)
        {
            return Result.Conflict("The Dungeon Master cannot be removed from the campaign; delete the campaign instead.");
        }

        // The secured repository authorises the delete against the member lock (Dungeon Master only);
        // a non-DM caller surfaces as a 403 via the global exception handler.
        await commandRepo.DeleteAsync(target, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
