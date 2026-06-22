using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;

/// <summary>
/// Accepts an invitation using its raw single-use token. The signed-in user's email must match the
/// invitee email, and the user joins the campaign as a Player.
/// </summary>
public sealed record AcceptInvitationCommand(string Token) : ICommand;
