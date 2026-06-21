using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.UpdateCampaign;

/// <summary>
/// Updates the name and description of a campaign. Only a Dungeon Master of the campaign may do so.
/// </summary>
public sealed record UpdateCampaignCommand(long Id, string Name, string? Description) : ICommand;
