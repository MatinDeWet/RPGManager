using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;

public sealed record AcceptInvitationCommand(string Token) : ICommand;
