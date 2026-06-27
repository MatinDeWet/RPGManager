using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.CreateCampaign;

/// <summary>
/// Creates a new campaign in a world owned by the current user. The creator is enrolled as the
/// campaign's first Dungeon Master.
/// </summary>
public sealed record CreateCampaignCommand(long WorldId, string Name, string? Description) : ICommand<long>;
