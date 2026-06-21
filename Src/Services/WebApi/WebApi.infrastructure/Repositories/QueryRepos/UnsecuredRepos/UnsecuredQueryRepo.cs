using Repository.Implementation;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.QueryRepos.UnsecuredRepos;

internal sealed class UnsecuredQueryRepo(CoreContext context)
    : QueryRepo<CoreContext>(context), IUnsecuredQueryRepo
{
    public IQueryable<User> Users => GetQueryable<User>();
}
