using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;

public sealed record GetCampaignMembersQuery(long CampaignId) : IQuery<IReadOnlyList<GetCampaignMembersResponse>>;
