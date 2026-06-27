using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using CQRS.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Pagination.Enums;
using Pagination.Models.Responses;
using WebApi.Application.Features.CampaignFeatures.SearchCampaigns;
using WebApi.Presentation.Common.Extensions;

namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

internal static class SearchCampaignsEndpoint
{
    public static RouteGroupBuilder MapSearchCampaignsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/", SearchCampaigns)
            .WithName("SearchCampaigns")
            .WithSummary("Returns a page of the current user's campaigns, optionally filtered by name and world.")
            .ProducesResult<PageableResponse<SearchCampaignsResponse>>();

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SearchCampaigns(
        [AsParameters] SearchCampaignsRequest request,
        [FromServices] IQueryManager<SearchCampaignsQuery, PageableResponse<SearchCampaignsResponse>> handler,
        CancellationToken cancellationToken)
    {
        SearchCampaignsQuery query = new()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            OrderDirection = request.OrderDirection,
            SearchTerm = request.SearchTerm,
            WorldId = request.WorldId,
        };

        Result<PageableResponse<SearchCampaignsResponse>> result = await handler.Handle(query, cancellationToken);

        return result.ToMinimalApiResult();
    }

    private readonly record struct SearchCampaignsRequest(
        [property: FromQuery] int PageNumber = 1,
        [property: FromQuery] int PageSize = 10,
        [property: FromQuery] string? OrderBy = null,
        [property: FromQuery] OrderDirectionEnum OrderDirection = OrderDirectionEnum.Ascending,
        [property: FromQuery] string? SearchTerm = null,
        [property: FromQuery] long? WorldId = null);
}
