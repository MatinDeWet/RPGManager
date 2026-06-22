using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

/// <summary>
/// Creates an email-bound, single-use invitation to a campaign. Only the Dungeon Master may invite.
/// </summary>
public sealed record CreateInvitationCommand(long CampaignId, string InviteeEmail) : ICommand<CreateInvitationResponse>;
