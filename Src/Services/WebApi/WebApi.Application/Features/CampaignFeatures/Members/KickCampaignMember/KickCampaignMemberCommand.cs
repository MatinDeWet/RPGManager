using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.KickCampaignMember;

public sealed record KickCampaignMemberCommand(long CampaignId, long UserId) : ICommand;
