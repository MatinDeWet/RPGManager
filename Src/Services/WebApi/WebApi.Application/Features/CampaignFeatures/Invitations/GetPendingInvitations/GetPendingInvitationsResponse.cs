namespace WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;

public sealed record GetPendingInvitationsResponse(long Id, string InviteeEmail, DateTimeOffset ExpiresAt, DateTimeOffset DateCreated);
