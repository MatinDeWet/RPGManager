using Ardalis.Result;
using CQRS.Core.Contracts;
using Pagination;
using Pagination.Models.Responses;
using Searchable.PostgreSQL;
using Searchable.PostgreSQL.Enums;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.SearchCampaigns;

internal sealed class SearchCampaignsQueryHandler(ICampaignSecuredQueryRepo queryRepo)
    : IQueryManager<SearchCampaignsQuery, PageableResponse<SearchCampaignsResponse>>
{
    public async Task<Result<PageableResponse<SearchCampaignsResponse>>> Handle(SearchCampaignsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Campaign> campaigns = queryRepo.Campaigns;

        if (request.WorldId.HasValue)
        {
            campaigns = campaigns.Where(x => x.WorldId == request.WorldId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            campaigns = campaigns.ILikeSearch(request, x => x.Name, ILikeMatchModeEnum.Contains);
        }

        PageableResponse<SearchCampaignsResponse> result = await campaigns
            .Select(x => new SearchCampaignsResponse
            {
                Id = x.Id,
                WorldId = x.WorldId,
                Name = x.Name,
                DateCreated = x.DateCreated,
            })
            .ToPageableListAsync(x => x.Id, request, cancellationToken);

        return result;
    }
}
