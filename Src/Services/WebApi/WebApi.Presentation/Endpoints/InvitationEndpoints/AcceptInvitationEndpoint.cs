using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;

namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

internal static class AcceptInvitationEndpoint
{
    public static RouteGroupBuilder MapAcceptInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{token}/accept", AcceptInvitation)
            .WithName("AcceptInvitation")
            .WithSummary("Accepts an invitation using its token. The signed-in user's email must match the invitee.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> AcceptInvitation(
        [FromRoute] string token,
        [FromServices] ICommandManager<AcceptInvitationCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new AcceptInvitationCommand(token), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
