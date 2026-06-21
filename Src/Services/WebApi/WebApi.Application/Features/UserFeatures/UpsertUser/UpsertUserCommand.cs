using CQRS.Core.Contracts;

namespace WebApi.Application.Features.UserFeatures.UpsertUser;

/// <summary>
/// Resolves the caller, identified by their external identity, to a persisted user — provisioning a
/// new record on the first login.
/// </summary>
public sealed record UpsertUserCommand(string ExternalId) : ICommand<UpsertUserResponse>;
