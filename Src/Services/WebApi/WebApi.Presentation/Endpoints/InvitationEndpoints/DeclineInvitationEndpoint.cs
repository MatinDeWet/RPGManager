using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.DeclineInvitation;

namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

internal static class DeclineInvitationEndpoint
{
    public static RouteGroupBuilder MapDeclineInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{token}/decline", DeclineInvitation)
            .WithName("DeclineInvitation")
            .WithSummary("Declines an invitation using its token. The signed-in user's email must match the invitee.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeclineInvitation(
        [FromRoute] string token,
        [FromServices] ICommandManager<DeclineInvitationCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeclineInvitationCommand(token), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
