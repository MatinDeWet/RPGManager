using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.CreateCampaign;

internal sealed class CreateCampaignCommandHandler(
    ISecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<CreateCampaignCommand, CreateCampaignResponse>
{
    public async Task<Result<CreateCampaignResponse>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        // Worlds is row-level filtered to the current user, so this also enforces world ownership.
        bool ownsWorld = await queryRepo.Worlds
            .AnyAsync(x => x.Id == request.WorldId, cancellationToken);

        if (!ownsWorld)
        {
            return Result.NotFound($"World '{request.WorldId}' was not found or is not owned by the current user.");
        }

        var campaign = Campaign.Create(request.WorldId, identityInfo.GetInternalUserId(), request.Name, request.Description);

        await commandRepo.InsertAsync(campaign, persistImmediately: true, cancellationToken);

        return new CreateCampaignResponse(campaign.Id);
    }
}
