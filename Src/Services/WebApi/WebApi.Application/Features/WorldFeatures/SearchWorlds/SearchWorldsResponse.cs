namespace WebApi.Application.Features.WorldFeatures.SearchWorlds;

// Member-init (not positional): EF Core cannot translate the ordering the pagination helper applies
// after a constructor projection, so search DTOs must be projected with member initialisers.
public sealed record SearchWorldsResponse
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateTimeOffset DateCreated { get; init; }
}
