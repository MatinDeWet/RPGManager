using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;

/// <summary>
/// Lists the members of a campaign the current user belongs to.
/// </summary>
public sealed record GetCampaignMembersQuery(long CampaignId) : IQuery<IReadOnlyList<GetCampaignMembersResponse>>;
