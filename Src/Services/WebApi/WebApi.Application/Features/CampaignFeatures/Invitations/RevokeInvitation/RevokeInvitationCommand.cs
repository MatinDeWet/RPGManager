using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.RevokeInvitation;

/// <summary>
/// Revokes a pending invitation. Only the Dungeon Master of the owning campaign may revoke.
/// </summary>
public sealed record RevokeInvitationCommand(long CampaignId, long InvitationId) : ICommand;
