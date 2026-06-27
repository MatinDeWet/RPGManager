using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;

namespace WebApi.Application.Features.WorldFeatures.CreateWorld;

internal sealed class CreateWorldCommandHandler(
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<CreateWorldCommand, long>
{
    public async Task<Result<long>> Handle(CreateWorldCommand request, CancellationToken cancellationToken)
    {
        var world = World.Create(identityInfo.GetInternalUserId(), request.Name, request.Description);

        await commandRepo.InsertAsync(world, persistImmediately: true, cancellationToken);

        return world.Id;
    }
}
