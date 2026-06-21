using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.UserFeatures.GetUser;

internal sealed class GetUserQueryHandler(ISecuredQueryRepo queryRepo)
    : IQueryManager<GetUserQuery, GetUserResponse>
{
    public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        GetUserResponse? user = await queryRepo.Users
            .Select(x => new GetUserResponse(x.Id, x.IdentityId, x.DateCreated))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.NotFound();
        }

        return user;
    }
}
