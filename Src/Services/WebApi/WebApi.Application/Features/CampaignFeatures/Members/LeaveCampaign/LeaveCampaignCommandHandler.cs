using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Members.LeaveCampaign;

internal sealed class LeaveCampaignCommandHandler(
    ICampaignMemberUnsecuredQueryRepo queryRepo,
    IUnsecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<LeaveCampaignCommand>
{
    public async Task<Result> Handle(LeaveCampaignCommand request, CancellationToken cancellationToken)
    {
        long userId = identityInfo.GetInternalUserId();

        CampaignMember? membership = await queryRepo.CampaignMembers
            .FirstOrDefaultAsync(x => x.CampaignId == request.CampaignId && x.UserId == userId, cancellationToken);

        if (membership is null)
        {
            return Result.NotFound($"You are not a member of campaign '{request.CampaignId}'.");
        }

        if (membership.Role == CampaignRole.DungeonMaster)
        {
            return Result.Conflict("The Dungeon Master cannot leave the campaign; delete the campaign instead.");
        }

        await commandRepo.DeleteAsync(membership, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
