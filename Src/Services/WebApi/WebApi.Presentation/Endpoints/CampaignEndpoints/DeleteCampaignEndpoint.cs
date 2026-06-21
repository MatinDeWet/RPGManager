using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.DeleteCampaign;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class DeleteCampaignEndpoint
{
    public static RouteGroupBuilder MapDeleteCampaignEndpoint(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}", DeleteCampaign)
            .WithName("DeleteCampaign")
            .WithSummary("Deletes a campaign. Requires the Dungeon Master role.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeleteCampaign(
        [FromRoute] long id,
        [FromServices] ICommandManager<DeleteCampaignCommand> handler,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteCampaignCommand(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
