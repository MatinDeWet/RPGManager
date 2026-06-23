using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface IWorldSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<World> Worlds { get; }
}
