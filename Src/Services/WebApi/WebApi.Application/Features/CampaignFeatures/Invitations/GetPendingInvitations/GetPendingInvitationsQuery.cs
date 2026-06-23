using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;

public sealed record GetPendingInvitationsQuery(long CampaignId) : IQuery<IReadOnlyList<GetPendingInvitationsResponse>>;
