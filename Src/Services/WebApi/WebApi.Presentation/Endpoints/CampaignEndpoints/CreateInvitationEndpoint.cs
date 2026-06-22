using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class CreateInvitationEndpoint
{
    public static RouteGroupBuilder MapCreateInvitationEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:long}/invitations", CreateInvitation)
            .WithName("CreateInvitation")
            .WithSummary("Creates an email-bound invitation to a campaign. Requires the Dungeon Master role.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateInvitation(
        [FromRoute] long id,
        [FromBody] CreateInvitationRequest request,
        [FromServices] ICommandManager<CreateInvitationCommand, CreateInvitationResponse> handler,
        CancellationToken cancellationToken)
    {
        CreateInvitationCommand command = new(id, request.InviteeEmail);

        Result<CreateInvitationResponse> result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record CreateInvitationRequest(string InviteeEmail);
}
