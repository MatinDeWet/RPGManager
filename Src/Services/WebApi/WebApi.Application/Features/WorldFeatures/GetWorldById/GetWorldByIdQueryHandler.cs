using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.WorldFeatures.GetWorldById;

internal sealed class GetWorldByIdQueryHandler(ISecuredQueryRepo queryRepo)
    : IQueryManager<GetWorldByIdQuery, GetWorldByIdResponse>
{
    public async Task<Result<GetWorldByIdResponse>> Handle(GetWorldByIdQuery request, CancellationToken cancellationToken)
    {
        GetWorldByIdResponse? world = await queryRepo.Worlds
            .Where(x => x.Id == request.Id)
            .Select(x => new GetWorldByIdResponse(x.Id, x.Name, x.Description, x.DateCreated))
            .FirstOrDefaultAsync(cancellationToken);

        if (world is null)
        {
            return Result.NotFound();
        }

        return world;
    }
}
