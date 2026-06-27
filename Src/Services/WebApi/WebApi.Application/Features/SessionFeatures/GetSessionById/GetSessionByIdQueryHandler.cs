using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.SessionFeatures.GetSessionById;

internal sealed class GetSessionByIdQueryHandler(ISessionSecuredQueryRepo queryRepo)
    : IQueryManager<GetSessionByIdQuery, GetSessionByIdResponse>
{
    public async Task<Result<GetSessionByIdResponse>> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        GetSessionByIdResponse? session = await queryRepo.Sessions
            .Where(x => x.Id == request.Id)
            .Select(x => new GetSessionByIdResponse(
                x.Id,
                x.CampaignId,
                x.SessionNumber,
                x.Title,
                x.ScheduledAt,
                x.Summary,
                x.Status,
                x.DateCreated))
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return Result.NotFound($"Session '{request.Id}' was not found or the current user is not a member of its campaign.");
        }

        return session;
    }
}
