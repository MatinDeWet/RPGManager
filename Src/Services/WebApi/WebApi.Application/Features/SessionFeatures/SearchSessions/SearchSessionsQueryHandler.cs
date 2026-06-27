using Ardalis.Result;
using CQRS.Core.Contracts;
using Pagination;
using Pagination.Models.Responses;
using Searchable.PostgreSQL;
using Searchable.PostgreSQL.Enums;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.SessionFeatures.SearchSessions;

internal sealed class SearchSessionsQueryHandler(ISessionSecuredQueryRepo queryRepo)
    : IQueryManager<SearchSessionsQuery, PageableResponse<SearchSessionsResponse>>
{
    public async Task<Result<PageableResponse<SearchSessionsResponse>>> Handle(SearchSessionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Session> sessions = queryRepo.Sessions;

        if (request.CampaignId.HasValue)
        {
            sessions = sessions.Where(x => x.CampaignId == request.CampaignId.Value);
        }

        if (request.Status.HasValue)
        {
            sessions = sessions.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            sessions = sessions.ILikeSearch(request, x => x.Title, ILikeMatchModeEnum.Contains);
        }

        PageableResponse<SearchSessionsResponse> result = await sessions
            .Select(x => new SearchSessionsResponse
            {
                Id = x.Id,
                CampaignId = x.CampaignId,
                SessionNumber = x.SessionNumber,
                Title = x.Title,
                ScheduledAt = x.ScheduledAt,
                Status = x.Status,
                DateCreated = x.DateCreated,
            })
            .ToPageableListAsync(x => x.Id, request, cancellationToken);

        return result;
    }
}
