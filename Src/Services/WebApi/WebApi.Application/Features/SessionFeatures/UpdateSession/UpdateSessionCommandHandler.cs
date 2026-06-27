using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.SessionFeatures.UpdateSession;

internal sealed class UpdateSessionCommandHandler(
    ISessionSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<UpdateSessionCommand>
{
    public async Task<Result> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        Session? session = await queryRepo.Sessions
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (session is null)
        {
            return Result.NotFound($"Session '{request.Id}' was not found or the current user is not a member of its campaign.");
        }

        session.Update(session.SessionNumber, request.Title, request.ScheduledAt, request.Summary);

        await commandRepo.UpdateAsync(session, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
