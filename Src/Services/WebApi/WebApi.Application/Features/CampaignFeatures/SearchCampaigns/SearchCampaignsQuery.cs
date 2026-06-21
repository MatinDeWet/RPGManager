using CQRS.Core.Contracts;
using Pagination.Models.Requests;
using Pagination.Models.Responses;
using Searchable.PostgreSQL.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.SearchCampaigns;

/// <summary>
/// Returns a page of the campaigns the current user is a member of, optionally filtered by an ILIKE
/// match on the name and/or by a specific world. The result set is row-level filtered to the current
/// user by the secured repository.
/// </summary>
public sealed class SearchCampaignsQuery : PageableRequest, IQuery<PageableResponse<SearchCampaignsResponse>>, ISearchableRequest
{
    public string? SearchTerm { get; init; }

    public long? WorldId { get; init; }
}
