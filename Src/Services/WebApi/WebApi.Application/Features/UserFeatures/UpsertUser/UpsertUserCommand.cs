using CQRS.Core.Contracts;

namespace WebApi.Application.Features.UserFeatures.UpsertUser;

public sealed record UpsertUserCommand(string ExternalId, string Email) : ICommand<UpsertUserResponse>;
