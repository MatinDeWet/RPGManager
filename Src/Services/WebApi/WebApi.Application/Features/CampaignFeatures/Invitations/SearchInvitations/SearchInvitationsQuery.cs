using CQRS.Core.Contracts;
using Pagination.Models.Requests;
using Pagination.Models.Responses;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.SearchInvitations;

public sealed class SearchInvitationsQuery : PageableRequest, IQuery<PageableResponse<SearchInvitationsResponse>>
{
    public long CampaignId { get; init; }
}
