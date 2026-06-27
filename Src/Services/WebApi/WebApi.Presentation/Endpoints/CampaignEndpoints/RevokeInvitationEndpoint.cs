using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.RevokeInvitation;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class RevokeInvitationEndpoint
{
    public static RouteGroupBuilder MapRevokeInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}/invitations/{invitationId:long}", RevokeInvitation)
            .WithName("RevokeInvitation")
            .WithSummary("Revokes a pending invitation. Requires the Dungeon Master role.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RevokeInvitation(
        [FromRoute] long id,
        [FromRoute] long invitationId,
        [FromServices] ICommandManager<RevokeInvitationCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new RevokeInvitationCommand(id, invitationId), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
