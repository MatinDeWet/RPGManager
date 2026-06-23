using Repository.Contracts;
using Shared.Domain.Entities;

namespace Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

public interface IUserUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<User> Users { get; }
}
