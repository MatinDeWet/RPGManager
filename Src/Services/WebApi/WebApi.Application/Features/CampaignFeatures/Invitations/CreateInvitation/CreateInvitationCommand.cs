using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

public sealed record CreateInvitationCommand(long CampaignId, string InviteeEmail) : ICommand<CreateInvitationResponse>;
