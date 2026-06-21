using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.DeleteCampaign;

internal sealed class DeleteCampaignCommandHandler(
    ISecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<DeleteCampaignCommand>
{
    public async Task<Result> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        Campaign? campaign = await queryRepo.Campaigns
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (campaign is null)
        {
            return Result.NotFound($"Campaign '{request.Id}' was not found or the current user is not a member of it.");
        }

        // The secured repository authorises the write against the campaign lock (Dungeon Master only);
        // a non-DM member surfaces as a 403 via the global exception handler.
        await commandRepo.DeleteAsync(campaign, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
