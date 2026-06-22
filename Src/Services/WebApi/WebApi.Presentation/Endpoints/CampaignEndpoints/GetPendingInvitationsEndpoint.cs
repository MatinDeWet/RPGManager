using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class GetPendingInvitationsEndpoint
{
    public static RouteGroupBuilder MapGetPendingInvitationsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}/invitations", GetPendingInvitations)
            .WithName("GetPendingInvitations")
            .WithSummary("Lists the pending invitations of a campaign. Requires the Dungeon Master role.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetPendingInvitations(
        [FromRoute] long id,
        [FromServices] IQueryManager<GetPendingInvitationsQuery, IReadOnlyList<GetPendingInvitationsResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GetPendingInvitationsResponse>> result = await handler.Handle(new GetPendingInvitationsQuery(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
