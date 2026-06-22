using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.KickCampaignMember;

/// <summary>
/// Removes a member from a campaign. Only the Dungeon Master may kick, and the Dungeon Master cannot
/// be targeted.
/// </summary>
public sealed record KickCampaignMemberCommand(long CampaignId, long UserId) : ICommand;
