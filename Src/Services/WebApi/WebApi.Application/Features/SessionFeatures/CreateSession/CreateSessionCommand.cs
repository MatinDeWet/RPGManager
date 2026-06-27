using CQRS.Core.Contracts;

namespace WebApi.Application.Features.SessionFeatures.CreateSession;

public sealed record CreateSessionCommand(long CampaignId, string Title, DateTimeOffset ScheduledAt, string? Summary) : ICommand<long>;
