using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface IUserSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<User> Users { get; }
}
