using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.UpdateCampaign;

internal sealed class UpdateCampaignCommandHandler(
    ICampaignSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<UpdateCampaignCommand>
{
    public async Task<Result> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        Campaign? campaign = await queryRepo.Campaigns
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (campaign is null)
        {
            return Result.NotFound($"Campaign '{request.Id}' was not found or the current user is not a member of it.");
        }

        campaign.Update(request.Name, request.Description);

        // The secured repository authorises the write against the campaign lock (Dungeon Master only);
        // a non-DM member surfaces as a 403 via the global exception handler.
        await commandRepo.UpdateAsync(campaign, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
