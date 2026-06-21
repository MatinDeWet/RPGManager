using Repository.Contracts;

namespace WebApi.Application.Repositories.CommandRepos.SecuredRepos;

/// <summary>
/// Secured command repository for the WebApi context. Validates the current user's permissions
/// against the registered entity protections before staging any modification.
/// </summary>
public interface ISecuredCommandRepo : ISecureCommandRepo;
