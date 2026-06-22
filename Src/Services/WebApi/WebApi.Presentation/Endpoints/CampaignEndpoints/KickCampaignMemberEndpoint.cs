using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Members.KickCampaignMember;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class KickCampaignMemberEndpoint
{
    public static RouteGroupBuilder MapKickCampaignMemberEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}/members/{userId:long}", KickCampaignMember)
            .WithName("KickCampaignMember")
            .WithSummary("Removes a member from a campaign. Requires the Dungeon Master role; the Dungeon Master cannot be removed.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> KickCampaignMember(
        [FromRoute] long id,
        [FromRoute] long userId,
        [FromServices] ICommandManager<KickCampaignMemberCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new KickCampaignMemberCommand(id, userId), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
