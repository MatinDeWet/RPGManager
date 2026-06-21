using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.GetCampaignById;

internal sealed class GetCampaignByIdQueryHandler(ISecuredQueryRepo queryRepo)
    : IQueryManager<GetCampaignByIdQuery, GetCampaignByIdResponse>
{
    public async Task<Result<GetCampaignByIdResponse>> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        GetCampaignByIdResponse? campaign = await queryRepo.Campaigns
            .Where(x => x.Id == request.Id)
            .Select(x => new GetCampaignByIdResponse(x.Id, x.WorldId, x.Name, x.Description, x.DateCreated))
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null)
        {
            return Result.NotFound($"Campaign '{request.Id}' was not found or the current user is not a member of it.");
        }

        return campaign;
    }
}
