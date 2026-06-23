namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

public sealed record CreateInvitationResponse(long Id, string Token, DateTimeOffset ExpiresAt);
