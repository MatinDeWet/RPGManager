using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.RevokeInvitation;

public sealed record RevokeInvitationCommand(long CampaignId, long InvitationId) : ICommand;
