using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.Members.LeaveCampaign;

/// <summary>
/// Removes the current user's own membership from a campaign. Only a Player may leave; the Dungeon
/// Master deletes the campaign instead.
/// </summary>
public sealed record LeaveCampaignCommand(long CampaignId) : ICommand;
