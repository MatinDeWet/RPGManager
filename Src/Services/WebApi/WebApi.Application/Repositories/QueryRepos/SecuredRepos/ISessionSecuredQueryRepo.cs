using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface ISessionSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<Session> Sessions { get; }
}
