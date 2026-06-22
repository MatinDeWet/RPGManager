using Ardalis.Result;
using CQRS.Core.Contracts;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Domain.Entities;
using WebApi.Application.Common.Options;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;

internal sealed class CreateInvitationCommandHandler(
    ICampaignSecuredQueryRepo queryRepo,
    ISecuredCommandRepo commandRepo,
    IIdentityInfo identityInfo,
    IOptions<InvitationOptions> options) : ICommandManager<CreateInvitationCommand, CreateInvitationResponse>
{
    public async Task<Result<CreateInvitationResponse>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        // Campaigns is row-level filtered to the current user; a campaign the caller is not a member
        // of surfaces as a 404. Whether the caller is the Dungeon Master is enforced by the
        // invitation lock on insert (a non-DM member surfaces as a 403).
        bool isMember = await queryRepo.Campaigns
            .AnyAsync(x => x.Id == request.CampaignId, cancellationToken);

        if (!isMember)
        {
            return Result.NotFound($"Campaign '{request.CampaignId}' was not found or the current user is not a member of it.");
        }

        (string rawToken, string tokenHash) = InvitationTokens.Generate();
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow + options.Value.Lifetime;

        var invitation = CampaignInvitation.Create(
            request.CampaignId,
            request.InviteeEmail,
            tokenHash,
            identityInfo.GetInternalUserId(),
            expiresAt);

        try
        {
            await commandRepo.InsertAsync(invitation, persistImmediately: true, cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The filtered unique index rejects a second pending invitation for the same campaign and email.
            return Result.Conflict("A pending invitation already exists for this email address.");
        }

        return new CreateInvitationResponse(invitation.Id, rawToken, invitation.ExpiresAt);
    }
}
