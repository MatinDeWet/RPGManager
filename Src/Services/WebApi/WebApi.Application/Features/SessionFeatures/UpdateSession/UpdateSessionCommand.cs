using CQRS.Core.Contracts;

namespace WebApi.Application.Features.SessionFeatures.UpdateSession;

public sealed record UpdateSessionCommand(long Id, string Title, DateTimeOffset ScheduledAt, string? Summary) : ICommand;
