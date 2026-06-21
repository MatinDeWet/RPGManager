using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.GetCampaignById;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class GetCampaignByIdEndpoint
{
    public static RouteGroupBuilder MapGetCampaignByIdEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}", GetCampaignById)
            .WithName("GetCampaignById")
            .WithSummary("Returns a single campaign the current user is a member of.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetCampaignById(
        [FromRoute] long id,
        [FromServices] IQueryManager<GetCampaignByIdQuery, GetCampaignByIdResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<GetCampaignByIdResponse> result = await handler.Handle(new GetCampaignByIdQuery(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
