using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.DeclineInvitation;

public sealed record DeclineInvitationCommand(string Token) : ICommand;
