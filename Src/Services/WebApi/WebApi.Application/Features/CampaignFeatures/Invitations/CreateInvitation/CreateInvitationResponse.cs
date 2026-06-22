namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

/// <summary>
/// The created invitation. <see cref="Token"/> is the raw single-use token, returned only here and
/// never stored — the caller must deliver it to the invitee.
/// </summary>
public sealed record CreateInvitationResponse(long Id, string Token, DateTimeOffset ExpiresAt);
