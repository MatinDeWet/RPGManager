using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

/// <summary>
/// Unsecured query repository for the WebApi context. Exposes entity queryables without row-level
/// security filtering — used for flows (such as login-time user provisioning) that run before an
/// internal user identity has been established.
/// </summary>
public interface IUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<User> Users { get; }
}
