using Repository.Implementation;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace Worker.infrastructure.Repositories.QueryRepos.UnsecuredRepos;

internal sealed class UserUnsecuredQueryRepo(CoreContext context)
    : QueryRepo<CoreContext>(context), IUserUnsecuredQueryRepo
{
    public IQueryable<User> Users => GetQueryable<User>();
}
