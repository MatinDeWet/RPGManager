using CQRS.Core.Contracts;
using Pagination.Models.Requests;
using Pagination.Models.Responses;
using Searchable.PostgreSQL.Contracts;
using Shared.Domain.Enums;

namespace WebApi.Application.Features.SessionFeatures.SearchSessions;

public sealed class SearchSessionsQuery : PageableRequest, IQuery<PageableResponse<SearchSessionsResponse>>, ISearchableRequest
{
    public string? SearchTerm { get; init; }

    public long? CampaignId { get; init; }

    public SessionStatus? Status { get; init; }
}
