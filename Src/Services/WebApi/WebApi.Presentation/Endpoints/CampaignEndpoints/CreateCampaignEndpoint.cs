using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.CreateCampaign;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class CreateCampaignEndpoint
{
    public static RouteGroupBuilder MapCreateCampaignEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreateCampaign)
            .WithName("CreateCampaign")
            .WithSummary("Creates a new campaign in a world owned by the current user.")
            .ProducesResult<CreateCampaignResponse>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateCampaign(
        [FromBody] CreateCampaignCommand command,
        [FromServices] ICommandManager<CreateCampaignCommand, CreateCampaignResponse> handler,
        CancellationToken cancellationToken)
    {
        Result<CreateCampaignResponse> result = await handler.Handle(command, cancellationToken);

        return result.ToMinimalApiResult();
    }
}
