namespace WebApi.Application.Features.CampaignFeatures.Invitations.SearchInvitations;

public sealed record SearchInvitationsResponse
{
    public long Id { get; init; }

    public string InviteeEmail { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset DateCreated { get; init; }
}
