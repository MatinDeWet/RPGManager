using Ardalis.Result;
using CQRS.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.RevokeInvitation;

internal sealed class RevokeInvitationCommandHandler(
    ICampaignInvitationSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo) : ICommandManager<RevokeInvitationCommand>
{
    public async Task<Result> Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        // Invitations is row-level filtered to campaigns the caller is the Dungeon Master of.
        CampaignInvitation? invitation = await queryRepo.Invitations
            .FirstOrDefaultAsync(x => x.Id == request.InvitationId && x.CampaignId == request.CampaignId, cancellationToken);

        if (invitation is null)
        {
            return Result.NotFound($"Invitation '{request.InvitationId}' was not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Conflict($"The invitation is '{invitation.Status}' and can no longer be revoked.");
        }

        invitation.Revoke();

        await commandRepo.UpdateAsync(invitation, persistImmediately: true, cancellationToken);

        return Result.Success();
    }
}
