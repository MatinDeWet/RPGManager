using Repository.Implementation;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace WebApi.infrastructure.Repositories.QueryRepos.UnsecuredRepos;

internal sealed class CampaignMemberUnsecuredQueryRepo(CoreContext context)
    : QueryRepo<CoreContext>(context), ICampaignMemberUnsecuredQueryRepo
{
    public IQueryable<CampaignMember> CampaignMembers => GetQueryable<CampaignMember>();
}
