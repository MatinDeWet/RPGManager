using CQRS.Core.Contracts;

namespace WebApi.Application.Features.UserFeatures.GetUser;

/// <summary>
/// Returns the currently authenticated user. The caller is resolved from the ambient identity and
/// the lookup is row-level filtered to that user by the secured repository.
/// </summary>
public sealed record GetUserQuery : IQuery<GetUserResponse>;
