using Ardalis.Result;
using CQRS.Core.Contracts;
using Pagination;
using Pagination.Models.Responses;
using Shared.Domain.Enums;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.SearchInvitations;

internal sealed class SearchInvitationsQueryHandler(ICampaignInvitationSecuredQueryRepo queryRepo)
    : IQueryManager<SearchInvitationsQuery, PageableResponse<SearchInvitationsResponse>>
{
    public async Task<Result<PageableResponse<SearchInvitationsResponse>>> Handle(SearchInvitationsQuery request, CancellationToken cancellationToken)
    {
        PageableResponse<SearchInvitationsResponse> result = await queryRepo.Invitations
            .Where(x => x.CampaignId == request.CampaignId && x.Status == InvitationStatus.Pending)
            .Select(x => new SearchInvitationsResponse
            {
                Id = x.Id,
                InviteeEmail = x.InviteeEmail,
                ExpiresAt = x.ExpiresAt,
                DateCreated = x.DateCreated,
            })
            .ToPageableListAsync(x => x.Id, request, cancellationToken);

        return result;
    }
}
