using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.DeclineInvitation;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

internal static class DeclineInvitationEndpoint
{
    public static RouteGroupBuilder MapDeclineInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/decline", DeclineInvitation)
            .WithName("DeclineInvitation")
            .WithSummary("Declines an invitation using its token. The signed-in user's email must match the invitee.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeclineInvitation(
        [FromBody] DeclineInvitationRequest request,
        [FromServices] ICommandManager<DeclineInvitationCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeclineInvitationCommand(request.Token), cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record DeclineInvitationRequest(string Token);
}
