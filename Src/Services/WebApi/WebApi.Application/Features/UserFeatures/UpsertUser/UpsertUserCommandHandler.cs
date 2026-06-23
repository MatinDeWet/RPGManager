using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;
using Shared.Domain.Entities;

namespace WebApi.Application.Features.UserFeatures.UpsertUser;

internal sealed class UpsertUserCommandHandler(
    IUserUnsecuredQueryRepo queryRepo,
    IUnsecuredCommandRepo commandRepo) : ICommandManager<UpsertUserCommand, UpsertUserResponse>
{
    public async Task<Result<UpsertUserResponse>> Handle(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await queryRepo.Users
            .FirstOrDefaultAsync(x => x.IdentityId == request.ExternalId, cancellationToken);

        if (user is not null)
        {
            if (user.Update(request.Email))
            {
                await commandRepo.UpdateAsync(user, persistImmediately: true, cancellationToken);
            }

            return new UpsertUserResponse(user.Id, user.IdentityId);
        }

        user = User.Create(request.ExternalId, request.Email);
        await commandRepo.InsertAsync(user, persistImmediately: true, cancellationToken);

        return new UpsertUserResponse(user.Id, user.IdentityId);
    }
}
