using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Members.LeaveCampaign;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class LeaveCampaignEndpoint
{
    public static RouteGroupBuilder MapLeaveCampaignEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}/members/me", LeaveCampaign)
            .WithName("LeaveCampaign")
            .WithSummary("Removes the current user's own membership from a campaign. The Dungeon Master cannot leave.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LeaveCampaign(
        [FromRoute] long id,
        [FromServices] ICommandManager<LeaveCampaignCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new LeaveCampaignCommand(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
