using CQRS.Core.Contracts;

namespace WebApi.Application.Features.SessionFeatures.GetSessionById;

public sealed record GetSessionByIdQuery(long Id) : IQuery<GetSessionByIdResponse>;
