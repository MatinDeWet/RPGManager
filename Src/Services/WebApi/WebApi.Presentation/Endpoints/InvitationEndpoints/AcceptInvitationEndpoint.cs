using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

internal static class AcceptInvitationEndpoint
{
    public static RouteGroupBuilder MapAcceptInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/accept", AcceptInvitation)
            .WithName("AcceptInvitation")
            .WithSummary("Accepts an invitation using its token. The signed-in user's email must match the invitee.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        [FromServices] ICommandManager<AcceptInvitationCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new AcceptInvitationCommand(request.Token), cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record AcceptInvitationRequest(string Token);
}
