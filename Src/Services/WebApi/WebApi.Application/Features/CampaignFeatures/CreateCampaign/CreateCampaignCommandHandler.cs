using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.CreateCampaign;

internal sealed class CreateCampaignCommandHandler(
    IWorldSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<CreateCampaignCommand, long>
{
    public async Task<Result<long>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        bool ownsWorld = await queryRepo.Worlds
            .AnyAsync(x => x.Id == request.WorldId, cancellationToken);

        if (!ownsWorld)
        {
            return Result.NotFound($"World '{request.WorldId}' was not found or is not owned by the current user.");
        }

        var campaign = Campaign.Create(request.WorldId, identityInfo.GetInternalUserId(), request.Name, request.Description);

        await commandRepo.InsertAsync(campaign, persistImmediately: true, cancellationToken);

        return campaign.Id;
    }
}
