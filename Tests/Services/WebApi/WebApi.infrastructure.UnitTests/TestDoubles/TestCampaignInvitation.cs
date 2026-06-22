using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="CampaignInvitation"/> rows for a campaign. <see cref="CampaignInvitation.Id"/>
/// has a protected setter (it is database-generated), so it is assigned via reflection for tests
/// that exercise the invitation lock directly.
/// </summary>
internal static class TestCampaignInvitation
{
    public static CampaignInvitation For(long id, long campaignId, string inviteeEmail = "invitee@example.com")
    {
        var invitation = CampaignInvitation.Create(
            campaignId,
            inviteeEmail,
            tokenHash: [1, 2, 3, 4],
            issuedByUserId: 1,
            expiresAt: DateTimeOffset.UtcNow.AddDays(7));

        typeof(CampaignInvitation).GetProperty(nameof(CampaignInvitation.Id))!.SetValue(invitation, id);

        return invitation;
    }
}
