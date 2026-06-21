using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.WorldFeatures.DeleteWorld;

internal sealed class DeleteWorldCommandHandler(
    ISecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<DeleteWorldCommand>
{
    public async Task<Result> Handle(DeleteWorldCommand request, CancellationToken cancellationToken)
    {
        World? world = await queryRepo.Worlds
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (world is null)
        {
            return Result.NotFound();
        }

        await commandRepo.DeleteAsync(world, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
