using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace WebApi.Application.UnitTests.TestDoubles;

/// <summary>
/// Builds domain entities with database-generated identifiers assigned via reflection, so handler
/// tests can exercise lookups by id without a real database.
/// </summary>
internal static class TestEntities
{
    public static Campaign Campaign(long id, long dungeonMasterUserId)
    {
        var campaign = Shared.Domain.Entities.Campaign.Create(worldId: 1, creatorUserId: dungeonMasterUserId, $"campaign-{id}", description: null);

        SetId(campaign, id);

        return campaign;
    }

    public static CampaignMember Member(long campaignId, long userId, CampaignRole role)
    {
        var member = CampaignMember.Create(userId, role);

        typeof(CampaignMember).GetProperty(nameof(CampaignMember.CampaignId))!.SetValue(member, campaignId);

        return member;
    }

    public static CampaignInvitation Invitation(
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

        SetId(invitation, id);

        return invitation;
    }

    private static void SetId<T>(T entity, long id)
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
    }
}
