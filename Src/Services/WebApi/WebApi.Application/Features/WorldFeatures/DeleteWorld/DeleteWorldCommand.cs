using CQRS.Core.Contracts;

namespace WebApi.Application.Features.WorldFeatures.DeleteWorld;

/// <summary>
/// Deletes a world owned by the current user.
/// </summary>
public sealed record DeleteWorldCommand(long Id) : ICommand;
