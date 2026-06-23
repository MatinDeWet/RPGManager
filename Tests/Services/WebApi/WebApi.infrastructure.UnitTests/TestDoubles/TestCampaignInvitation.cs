using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

internal static class TestCampaignInvitation
{
    public static CampaignInvitation For(long id, long campaignId, string inviteeEmail = "invitee@example.com")
    {
        var invitation = CampaignInvitation.Create(
            campaignId,
            inviteeEmail,
            tokenHash: $"hash-{id}",
            issuedByUserId: 1,
            expiresAt: DateTimeOffset.UtcNow.AddDays(7));

        typeof(CampaignInvitation).GetProperty(nameof(CampaignInvitation.Id))!.SetValue(invitation, id);

        return invitation;
    }
}
