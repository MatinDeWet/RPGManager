using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class GetCampaignMembersEndpoint
{
    public static RouteGroupBuilder MapGetCampaignMembersEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}/members", GetCampaignMembers)
            .WithName("GetCampaignMembers")
            .WithSummary("Lists the members of a campaign the current user is a member of.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetCampaignMembers(
        [FromRoute] long id,
        [FromServices] IQueryManager<GetCampaignMembersQuery, IReadOnlyList<GetCampaignMembersResponse>> handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GetCampaignMembersResponse>> result = await handler.Handle(new GetCampaignMembersQuery(id), cancellationToken);

        return result.ToMinimalApiResult();
    }
}
