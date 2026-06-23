using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Pagination.Enums;
using Pagination.Models.Responses;
using WebApi.Application.Features.CampaignFeatures.Invitations.SearchInvitations;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class SearchInvitationsEndpoint
{
    public static RouteGroupBuilder MapSearchInvitationsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}/invitations", SearchInvitations)
            .WithName("SearchInvitations")
            .WithSummary("Returns a page of the pending invitations of a campaign. Requires the Dungeon Master role.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SearchInvitations(
        [FromRoute] long id,
        [AsParameters] SearchInvitationsRequest request,
        [FromServices] IQueryManager<SearchInvitationsQuery, PageableResponse<SearchInvitationsResponse>> handler,
        CancellationToken cancellationToken)
    {
        SearchInvitationsQuery query = new()
        {
            CampaignId = id,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            OrderDirection = request.OrderDirection,
        };

        Result<PageableResponse<SearchInvitationsResponse>> result = await handler.Handle(query, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private readonly record struct SearchInvitationsRequest(
        [property: FromQuery] int PageNumber = 1,
        [property: FromQuery] int PageSize = 10,
        [property: FromQuery] string? OrderBy = null,
        [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending);
}
