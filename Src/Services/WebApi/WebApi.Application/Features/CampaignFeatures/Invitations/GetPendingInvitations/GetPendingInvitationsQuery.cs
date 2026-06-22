using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;

/// <summary>
/// Lists the pending invitations of a campaign. Only the Dungeon Master sees them.
/// </summary>
public sealed record GetPendingInvitationsQuery(long CampaignId) : IQuery<IReadOnlyList<GetPendingInvitationsResponse>>;
