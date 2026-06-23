using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface ICampaignSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<Campaign> Campaigns { get; }
}
