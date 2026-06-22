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
        // Campaigns is row-level filtered to the current user, so members are only returned for a
        // campaign the caller is a member of. A visible campaign always has at least the Dungeon
        // Master, so an empty result means the campaign is not visible to the caller.
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
