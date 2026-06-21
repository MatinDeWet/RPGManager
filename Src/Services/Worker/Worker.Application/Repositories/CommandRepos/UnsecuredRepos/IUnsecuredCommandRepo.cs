using Repository.Contracts;

namespace Worker.Application.Repositories.CommandRepos.UnsecuredRepos;

/// <summary>
/// Unsecured command repository for the worker. Stages modifications without permission validation —
/// background jobs run as the system, with no current-user identity.
/// </summary>
public interface IUnsecuredCommandRepo : ICommandRepo;
