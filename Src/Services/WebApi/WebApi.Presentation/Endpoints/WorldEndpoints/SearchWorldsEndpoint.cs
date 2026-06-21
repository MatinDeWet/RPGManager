using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Pagination.Enums;
using Pagination.Models.Responses;
using WebApi.Application.Features.WorldFeatures.SearchWorlds;

namespace WebApi.Presentation.Endpoints.WorldEndpoints;

internal static class SearchWorldsEndpoint
{
    public static RouteGroupBuilder MapSearchWorldsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", SearchWorlds)
            .WithName("SearchWorlds")
            .WithSummary("Returns a page of the current user's worlds, optionally filtered by name.");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SearchWorlds(
        [AsParameters] SearchWorldsRequest request,
        [FromServices] IQueryManager<SearchWorldsQuery, PageableResponse<SearchWorldsResponse>> handler,
        CancellationToken cancellationToken)
    {
        SearchWorldsQuery query = new()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            OrderDirection = request.OrderDirection,
            SearchTerm = request.SearchTerm,
        };

        Result<PageableResponse<SearchWorldsResponse>> result = await handler.Handle(query, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private readonly record struct SearchWorldsRequest(
        [property: FromQuery] int PageNumber = 1,
        [property: FromQuery] int PageSize = 10,
        [property: FromQuery] string? OrderBy = null,
        [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending,
        [property: FromQuery] string? SearchTerm = null);
}
