using Repository.Contracts;

namespace WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;

/// <summary>
/// Unsecured command repository for the WebApi context. Stages modifications without permission
/// validation — used for flows (such as login-time user provisioning) that run before an internal
/// user identity has been established.
/// </summary>
public interface IUnsecuredCommandRepo : ICommandRepo;
