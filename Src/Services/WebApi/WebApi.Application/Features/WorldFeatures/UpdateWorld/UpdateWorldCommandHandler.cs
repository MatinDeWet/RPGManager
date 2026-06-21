using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.WorldFeatures.UpdateWorld;

internal sealed class UpdateWorldCommandHandler(
    ISecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<UpdateWorldCommand>
{
    public async Task<Result> Handle(UpdateWorldCommand request, CancellationToken cancellationToken)
    {
        World? world = await queryRepo.Worlds
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (world is null)
        {
            return Result.NotFound($"World '{request.Id}' was not found or is not owned by the current user.");
        }

        world.Update(request.Name, request.Description);

        await commandRepo.UpdateAsync(world, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
