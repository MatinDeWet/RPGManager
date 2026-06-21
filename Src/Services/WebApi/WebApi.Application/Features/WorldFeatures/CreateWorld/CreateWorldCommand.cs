using CQRS.Core.Contracts;

namespace WebApi.Application.Features.WorldFeatures.CreateWorld;

/// <summary>
/// Creates a new world owned by the current user. The owner is resolved from the ambient identity.
/// </summary>
public sealed record CreateWorldCommand(string Name, string? Description) : ICommand<CreateWorldResponse>;
