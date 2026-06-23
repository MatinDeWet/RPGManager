using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.DeleteCampaign;

internal sealed class DeleteCampaignCommandHandler(
    ICampaignSecuredQueryRepo queryRepo,
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

        await commandRepo.DeleteAsync(campaign, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
