using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace WebApi.Application.UnitTests.TestDoubles;

internal static class TestCampaignInvitation
{
    public static CampaignInvitation Create(
        long id,
        long campaignId,
        string inviteeEmail,
        string tokenHash,
        DateTimeOffset expiresAt,
        InvitationStatus status = InvitationStatus.Pending)
    {
        var invitation = CampaignInvitation.Create(campaignId, inviteeEmail, tokenHash, issuedByUserId: 1, expiresAt);

        switch (status)
        {
            case InvitationStatus.Accepted:
                invitation.Accept(userId: 99);
                break;
            case InvitationStatus.Declined:
                invitation.Decline();
                break;
            case InvitationStatus.Revoked:
                invitation.Revoke();
                break;
            case InvitationStatus.Pending:
            default:
                break;
        }

        TestEntity.SetId(invitation, id);

        return invitation;
    }
}
