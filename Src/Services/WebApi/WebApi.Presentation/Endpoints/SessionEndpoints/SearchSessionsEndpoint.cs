using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Pagination.Enums;
using Pagination.Models.Responses;
using Shared.Domain.Enums;
using WebApi.Application.Features.SessionFeatures.SearchSessions;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.SessionEndpoints;

internal static class SearchSessionsEndpoint
{
    public static RouteGroupBuilder MapSearchSessionsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", SearchSessions)
            .WithName("SearchSessions")
            .WithSummary("Returns a page of the sessions the current user can see, optionally filtered by campaign, status and title.")
            .ProducesResult<PageableResponse<SearchSessionsResponse>>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SearchSessions(
        [AsParameters] SearchSessionsRequest request,
        [FromServices] IQueryManager<SearchSessionsQuery, PageableResponse<SearchSessionsResponse>> handler,
        CancellationToken cancellationToken)
    {
        SearchSessionsQuery query = new()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            OrderDirection = request.OrderDirection,
            SearchTerm = request.SearchTerm,
            CampaignId = request.CampaignId,
            Status = request.Status,
        };

        Result<PageableResponse<SearchSessionsResponse>> result = await handler.Handle(query, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private readonly record struct SearchSessionsRequest(
        [property: FromQuery] int PageNumber = 1,
        [property: FromQuery] int PageSize = 10,
        [property: FromQuery] string? OrderBy = null,
        [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending,
        [property: FromQuery] string? SearchTerm = null,
        [property: FromQuery] long? CampaignId = null,
        [property: FromQuery] SessionStatus? Status = null);
}
