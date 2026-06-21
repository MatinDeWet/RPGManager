using CQRS.Core.Contracts;

namespace WebApi.Application.Features.WorldFeatures.UpdateWorld;

/// <summary>
/// Updates the name and description of a world owned by the current user.
/// </summary>
public sealed record UpdateWorldCommand(long Id, string Name, string? Description) : ICommand;
