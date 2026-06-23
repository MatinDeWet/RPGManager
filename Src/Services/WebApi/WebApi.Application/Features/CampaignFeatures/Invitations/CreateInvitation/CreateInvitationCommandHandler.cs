using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using WebApi.Application.Common.Options;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

internal sealed class CreateInvitationCommandHandler(
    ICampaignSecuredQueryRepo campaignQueryRepo,
    ICampaignMemberSecuredQueryRepo memberQueryRepo,
    ICampaignInvitationSecuredQueryRepo invitationQueryRepo,
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo,
    IOptions<InvitationOptions> options) : ICommandManager<CreateInvitationCommand, CreateInvitationResponse>
{
    public async Task<Result<CreateInvitationResponse>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        bool isMember = await campaignQueryRepo.Campaigns
            .AnyAsync(x => x.Id == request.CampaignId, cancellationToken);

        if (!isMember)
        {
            return Result.NotFound($"Campaign '{request.CampaignId}' was not found or the current user is not a member of it.");
        }

        string normalizedEmail = CampaignInvitation.NormalizeEmail(request.InviteeEmail);

        bool inviteeAlreadyMember = await memberQueryRepo.CampaignMembers
            .AnyAsync(x => x.CampaignId == request.CampaignId && x.User.Email == normalizedEmail, cancellationToken);

        if (inviteeAlreadyMember)
        {
            return Result.Conflict("This email belongs to a user who is already a member of the campaign.");
        }

        CampaignInvitation? existing = await invitationQueryRepo.Invitations
            .FirstOrDefaultAsync(
                x => x.CampaignId == request.CampaignId
                    && x.InviteeEmail == normalizedEmail
                    && x.Status == InvitationStatus.Pending,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return Result.Conflict("A pending invitation already exists for this email address.");
            }

            existing.Revoke();
            await commandRepo.UpdateAsync(existing, cancellationToken);
        }

        (string rawToken, string tokenHash) = InvitationTokens.Generate();
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow + options.Value.Lifetime;

        var invitation = CampaignInvitation.Create(
            request.CampaignId,
            request.InviteeEmail,
            tokenHash,
            identityInfo.GetInternalUserId(),
            expiresAt);

        await commandRepo.InsertAsync(invitation, cancellationToken);

        try
        {
            await commandRepo.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await HasPendingInvitationAsync(request.CampaignId, normalizedEmail, cancellationToken))
            {
                return Result.Conflict("A pending invitation already exists for this email address.");
            }

            throw;
        }

        return new CreateInvitationResponse(invitation.Id, rawToken, invitation.ExpiresAt);
    }

    private async Task<bool> HasPendingInvitationAsync(long campaignId, string normalizedEmail, CancellationToken cancellationToken)
    {
        return await invitationQueryRepo.Invitations
            .AnyAsync(
                x => x.CampaignId == campaignId
                    && x.InviteeEmail == normalizedEmail
                    && x.Status == InvitationStatus.Pending,
                cancellationToken);
    }
}
