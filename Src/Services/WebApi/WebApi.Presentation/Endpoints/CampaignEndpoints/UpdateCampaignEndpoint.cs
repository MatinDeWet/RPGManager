using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.UpdateCampaign;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class UpdateCampaignEndpoint
{
    public static RouteGroupBuilder MapUpdateCampaignEndpoint(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:long}", UpdateCampaign)
            .WithName("UpdateCampaign")
            .WithSummary("Updates the name and description of a campaign. Requires the Dungeon Master role.")
            .ProducesResult();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateCampaign(
        [FromRoute] long id,
        [FromBody] UpdateCampaignRequest request,
        [FromServices] ICommandManager<UpdateCampaignCommand> handler,
        CancellationToken cancellationToken)
    {
        UpdateCampaignCommand command = new(id, request.Name, request.Description);

        Result result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private sealed record UpdateCampaignRequest(string Name, string? Description);
}
