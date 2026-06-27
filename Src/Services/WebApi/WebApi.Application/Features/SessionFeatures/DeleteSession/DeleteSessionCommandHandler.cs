using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.SessionFeatures.DeleteSession;

internal sealed class DeleteSessionCommandHandler(
    ISessionSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<DeleteSessionCommand>
{
    public async Task<Result> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        Session? session = await queryRepo.Sessions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (session is null)
        {
            return Result.NotFound($"Session '{request.Id}' was not found or the current user is not a member of its campaign.");
        }

        await commandRepo.DeleteAsync(session, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
