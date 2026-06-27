using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.SessionFeatures.CreateSession;

internal sealed class CreateSessionCommandHandler(
    ICampaignSecuredQueryRepo campaignRepo,
    ISessionSecuredQueryRepo sessionRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<Result<CreateSessionResponse>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        bool campaignVisible = await campaignRepo.Campaigns
            .AnyAsync(x => x.Id == request.CampaignId, cancellationToken);

        if (!campaignVisible)
        {
            return Result.NotFound($"Campaign '{request.CampaignId}' was not found or the current user is not a member of it.");
        }

        int nextNumber = (await sessionRepo.Sessions
            .Where(x => x.CampaignId == request.CampaignId)
            .Select(x => (int?)x.SessionNumber)
            .MaxAsync(cancellationToken) ?? 0) + 1;

        var session = Session.Create(request.CampaignId, nextNumber, request.Title, request.ScheduledAt, request.Summary);

        await commandRepo.InsertAsync(session, persistImmediately: true, cancellationToken);

        return new CreateSessionResponse(session.Id);
    }
}
