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
    ICampaignInvitationSecuredQueryRepo invitationQueryRepo,
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo,
    IOptions<InvitationOptions> options) : ICommandManager<CreateInvitationCommand, CreateInvitationResponse>
{
    public async Task<Result<CreateInvitationResponse>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        // Campaigns is row-level filtered to the current user; a campaign the caller is not a member
        // of surfaces as a 404. Whether the caller is the Dungeon Master is enforced by the
        // invitation lock on insert (a non-DM member surfaces as a 403).
        bool isMember = await campaignQueryRepo.Campaigns
            .AnyAsync(x => x.Id == request.CampaignId, cancellationToken);

        if (!isMember)
        {
            return Result.NotFound($"Campaign '{request.CampaignId}' was not found or the current user is not a member of it.");
        }

        // Expiry is derived (there is no stored Expired state), so an expired-but-still-pending
        // invitation keeps occupying the one-pending-per-(campaign, email) slot enforced by the
        // filtered unique index. Revoke such a stale row so a fresh invitation can be issued;
        // a still-valid pending invitation is a genuine conflict. Invitations is filtered to
        // campaigns the caller is the Dungeon Master of, matching the create authorisation.
        string normalizedEmail = CampaignInvitation.NormalizeEmail(request.InviteeEmail);

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
            // Commits the stale-invitation revoke (if any) and the new invitation together.
            await commandRepo.SaveAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The only expected failure is a concurrent request having created a pending invitation for
            // the same campaign and email after our check, which the filtered unique index rejected. Any
            // other update failure is unexpected and propagates.
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
