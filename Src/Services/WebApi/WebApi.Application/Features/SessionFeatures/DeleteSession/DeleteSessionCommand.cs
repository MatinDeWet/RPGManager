using CQRS.Core.Contracts;

namespace WebApi.Application.Features.SessionFeatures.DeleteSession;

public sealed record DeleteSessionCommand(long Id) : ICommand;
