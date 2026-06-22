using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

public interface ICampaignMemberUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<CampaignMember> CampaignMembers { get; }
}
