namespace WebApi.Application.Features.CampaignFeatures.SearchCampaigns;

// Member-init (not positional): EF Core cannot translate the ordering the pagination helper applies
// after a constructor projection, so search DTOs must be projected with member initialisers.
public sealed record SearchCampaignsResponse
{
    public long Id { get; init; }

    public long WorldId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTimeOffset DateCreated { get; init; }
}
