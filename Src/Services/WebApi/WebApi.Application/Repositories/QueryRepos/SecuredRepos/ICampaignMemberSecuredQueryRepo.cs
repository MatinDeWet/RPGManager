using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface ICampaignMemberSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<CampaignMember> CampaignMembers { get; }
}
