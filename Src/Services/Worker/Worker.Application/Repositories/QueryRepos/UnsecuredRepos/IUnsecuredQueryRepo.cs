using Repository.Contracts;
using Shared.Domain.Entities;

namespace Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

/// <summary>
/// Unsecured query repository for the worker. Background jobs run with no current-user identity, so
/// they read through these entity queryables without any row-level security filtering.
/// </summary>
public interface IUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<User> Users { get; }
}
