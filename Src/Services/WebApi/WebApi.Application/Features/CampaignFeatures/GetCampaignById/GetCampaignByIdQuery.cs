using CQRS.Core.Contracts;

namespace WebApi.Application.Features.CampaignFeatures.GetCampaignById;

/// <summary>
/// Returns a single campaign the current user is a member of. The lookup is row-level filtered by the
/// secured repository, so a campaign the user does not belong to resolves to not found.
/// </summary>
public sealed record GetCampaignByIdQuery(long Id) : IQuery<GetCampaignByIdResponse>;
