using Identification.Contracts;
using Repository.Contracts;
using Repository.Implementation;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.infrastructure.Repositories.QueryRepos.SecuredRepos;

internal sealed class CampaignSecuredQueryRepo(CoreContext context, IIdentityInfo info, IEnumerable<IProtected> protection)
    : SecureQueryRepo<CoreContext>(context, info, protection), ICampaignSecuredQueryRepo
{
    public IQueryable<Campaign> Campaigns => GetQueryable<Campaign>();
}
