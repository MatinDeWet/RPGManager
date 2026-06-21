using Ardalis.Result;
using CQRS.Core.Contracts;
using Pagination;
using Pagination.Models.Responses;
using Searchable.PostgreSQL;
using Searchable.PostgreSQL.Enums;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.WorldFeatures.SearchWorlds;

internal sealed class SearchWorldsQueryHandler(ISecuredQueryRepo queryRepo)
    : IQueryManager<SearchWorldsQuery, PageableResponse<SearchWorldsResponse>>
{
    public async Task<Result<PageableResponse<SearchWorldsResponse>>> Handle(SearchWorldsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<World> worlds = queryRepo.Worlds;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            worlds = worlds.ILikeSearch(request, x => x.Name, ILikeMatchModeEnum.Contains);
        }

        PageableResponse<SearchWorldsResponse> result = await worlds
            .Select(x => new SearchWorldsResponse
            {
                Id = x.Id,
                Name = x.Name,
                DateCreated = x.DateCreated,
            })
            .ToPageableListAsync(x => x.Id, request, cancellationToken);

        return result;
    }
}
