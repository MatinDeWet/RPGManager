using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Enums;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;

internal sealed class GetPendingInvitationsQueryHandler(ICampaignInvitationSecuredQueryRepo queryRepo)
    : IQueryManager<GetPendingInvitationsQuery, IReadOnlyList<GetPendingInvitationsResponse>>
{
    public async Task<Result<IReadOnlyList<GetPendingInvitationsResponse>>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        List<GetPendingInvitationsResponse> invitations = await queryRepo.Invitations
            .Where(x => x.CampaignId == request.CampaignId && x.Status == InvitationStatus.Pending)
            .Select(x => new GetPendingInvitationsResponse(x.Id, x.InviteeEmail, x.ExpiresAt, x.DateCreated))
            .ToListAsync(cancellationToken);

        return invitations;
    }
}
