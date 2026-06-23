using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

public interface IUserUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<User> Users { get; }
}
