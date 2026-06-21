using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.DeleteCampaign;

/// <summary>
/// Deletes a campaign. Only a Dungeon Master of the campaign may do so.
/// </summary>
public sealed record DeleteCampaignCommand(long Id) : ICommand;
