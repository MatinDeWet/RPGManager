namespace WebApi.Application.Features.WorldFeatures.GetWorldById;

public sealed record GetWorldByIdResponse(long Id, string Name, string? Description, DateTimeOffset DateCreated);
