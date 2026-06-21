using CQRS.Core.Contracts;
using Pagination.Models.Requests;
using Pagination.Models.Responses;
using Searchable.PostgreSQL.Contracts;

namespace WebApi.Application.Features.WorldFeatures.SearchWorlds;

/// <summary>
/// Returns a page of the current user's worlds, optionally filtered by an ILIKE match on the name.
/// The result set is row-level filtered to the current user by the secured repository.
/// </summary>
public sealed class SearchWorldsQuery : PageableRequest, IQuery<PageableResponse<SearchWorldsResponse>>, ISearchableRequest
{
    public string? SearchTerm { get; init; }
}
