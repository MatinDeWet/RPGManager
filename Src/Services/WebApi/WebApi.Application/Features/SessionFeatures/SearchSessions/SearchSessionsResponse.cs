using Shared.Domain.Enums;

namespace WebApi.Application.Features.SessionFeatures.SearchSessions;

public sealed record SearchSessionsResponse
{
    public long Id { get; init; }

    public long CampaignId { get; init; }

    public int SessionNumber { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTimeOffset ScheduledAt { get; init; }

    public SessionStatus Status { get; init; }

    public DateTimeOffset DateCreated { get; init; }
}
