using Shared.Domain.Enums;

namespace WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;

public sealed record GetCampaignMembersResponse(long UserId, CampaignRole Role, DateTimeOffset JoinedAt);
