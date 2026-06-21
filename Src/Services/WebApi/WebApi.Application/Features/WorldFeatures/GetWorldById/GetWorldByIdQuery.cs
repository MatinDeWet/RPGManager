using CQRS.Core.Contracts;

namespace WebApi.Application.Features.WorldFeatures.GetWorldById;

/// <summary>
/// Returns a single world owned by the current user. The lookup is row-level filtered to that user
/// by the secured repository, so a world owned by anyone else resolves to not found.
/// </summary>
public sealed record GetWorldByIdQuery(long Id) : IQuery<GetWorldByIdResponse>;
