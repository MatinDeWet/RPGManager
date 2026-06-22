using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.DeclineInvitation;

/// <summary>
/// Declines an invitation using its raw single-use token. The signed-in user's email must match the
/// invitee email.
/// </summary>
public sealed record DeclineInvitationCommand(string Token) : ICommand;
