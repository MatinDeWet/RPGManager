using Shared.Domain.Enums;

namespace WebApi.Application.Features.SessionFeatures.GetSessionById;

public sealed record GetSessionByIdResponse(
    long Id,
    long CampaignId,
    int SessionNumber,
    string Title,
    DateTimeOffset ScheduledAt,
    string? Summary,
    SessionStatus Status,
    DateTimeOffset DateCreated);
