using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;

internal sealed class GetCampaignMembersQueryHandler(ICampaignSecuredQueryRepo queryRepo)
    : IQueryManager<GetCampaignMembersQuery, IReadOnlyList<GetCampaignMembersResponse>>
{
    public async Task<Result<IReadOnlyList<GetCampaignMembersResponse>>> Handle(GetCampaignMembersQuery request, CancellationToken cancellationToken)
    {
        List<GetCampaignMembersResponse> members = await queryRepo.Campaigns
            .Where(x => x.Id == request.CampaignId)
            .SelectMany(x => x.Members)
            .Select(x => new GetCampaignMembersResponse(x.UserId, x.Role, x.DateCreated))
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
        {
            return Result.NotFound($"Campaign '{request.CampaignId}' was not found or the current user is not a member of it.");
        }

        return members;
    }
}
