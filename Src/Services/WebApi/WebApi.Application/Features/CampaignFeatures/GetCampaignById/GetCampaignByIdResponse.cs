namespace WebApi.Application.Features.CampaignFeatures.GetCampaignById;

public sealed record GetCampaignByIdResponse(long Id, long WorldId, string Name, string? Description, DateTimeOffset DateCreated);
