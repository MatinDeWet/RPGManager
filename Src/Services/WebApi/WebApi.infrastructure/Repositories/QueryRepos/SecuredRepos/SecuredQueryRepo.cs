using Identification.Contracts;
using Repository.Contracts;
using Repository.Implementation;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.QueryRepos.SecuredRepos;

internal sealed class SecuredQueryRepo(CoreContext context, IIdentityInfo info, IEnumerable<IProtected> protection)
    : SecureQueryRepo<CoreContext>(context, info, protection), ISecuredQueryRepo
{
    public IQueryable<User> Users => GetQueryable<User>();

    public IQueryable<World> Worlds => GetQueryable<World>();

    public IQueryable<Campaign> Campaigns => GetQueryable<Campaign>();
}
