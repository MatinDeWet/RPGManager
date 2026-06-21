using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

/// <summary>
/// Secured query repository for the WebApi context. Exposes entity queryables that are always
/// row-level filtered to the current user via the registered entity protections.
/// </summary>
public interface ISecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<User> Users { get; }

    IQueryable<World> Worlds { get; }
}
