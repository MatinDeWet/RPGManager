using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Constants;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler(
    ICampaignInvitationUnsecuredQueryRepo invitationQueryRepo,
    ICampaignMemberUnsecuredQueryRepo memberQueryRepo,
    IUnsecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo) : ICommandManager<AcceptInvitationCommand>
{
    public async Task<Result> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = InvitationTokens.Hash(request.Token);

        // The invitee is not yet a member, so this runs unsecured; the raw token is the credential.
        CampaignInvitation? invitation = await invitationQueryRepo.Invitations
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (invitation is null)
        {
            return Result.NotFound("The invitation was not found.");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Conflict($"The invitation is '{invitation.Status}' and can no longer be accepted.");
        }

        if (invitation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Result.Conflict("The invitation has expired.");
        }

        // The caller must prove control of the invited address: a present, IdP-verified email claim
        // that matches the invitee. An unverified or absent claim fails closed.
        string email = identityInfo.GetValue(ClaimConstants.Email);
        bool emailVerified = bool.TryParse(identityInfo.GetValue(ClaimConstants.EmailVerified), out bool verified) && verified;

        if (!emailVerified || string.IsNullOrWhiteSpace(email) || CampaignInvitation.NormalizeEmail(email) != invitation.InviteeEmail)
        {
            return Result.Forbidden();
        }

        long userId = identityInfo.GetInternalUserId();

        bool alreadyMember = await memberQueryRepo.CampaignMembers
            .AnyAsync(x => x.CampaignId == invitation.CampaignId && x.UserId == userId, cancellationToken);

        if (alreadyMember)
        {
            return Result.Conflict("You are already a member of this campaign.");
        }

        invitation.Accept(userId);
        await commandRepo.UpdateAsync(invitation, cancellationToken);
        await commandRepo.InsertAsync(CampaignMember.Create(invitation.CampaignId, userId, CampaignRole.Player), cancellationToken);

        await commandRepo.SaveAsync(cancellationToken);

        return Result.Success();
    }
}
