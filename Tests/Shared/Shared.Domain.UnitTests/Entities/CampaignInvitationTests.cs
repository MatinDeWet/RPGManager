using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Xunit;

namespace Shared.Domain.UnitTests.Entities;

public class CampaignInvitationTests
{
    private static readonly byte[] Hash = [1, 2, 3, 4];
    private static readonly DateTimeOffset Expiry = DateTimeOffset.UtcNow.AddDays(7);

    private static CampaignInvitation Pending(string email = "invitee@example.com")
    {
        return CampaignInvitation.Create(campaignId: 1, email, Hash, issuedByUserId: 10, Expiry);
    }

    [Fact]
    public void Create_NormalizesEmail_AndStartsPending()
    {
        var invite = CampaignInvitation.Create(campaignId: 1, "  Invitee@Example.COM ", Hash, issuedByUserId: 10, Expiry);

        invite.InviteeEmail.ShouldBe("invitee@example.com");
        invite.Status.ShouldBe(InvitationStatus.Pending);
        invite.CampaignId.ShouldBe(1);
        invite.IssuedByUserId.ShouldBe(10);
        invite.ExpiresAt.ShouldBe(Expiry);
        invite.AcceptedByUserId.ShouldBeNull();
        invite.RespondedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Throws_ForInvalidEmail(string? email)
    {
        Should.Throw<ArgumentException>(() => CampaignInvitation.Create(1, email!, Hash, 10, Expiry));
    }

    [Fact]
    public void Create_Throws_ForEmptyTokenHash()
    {
        Should.Throw<ArgumentException>(() => CampaignInvitation.Create(1, "a@b.com", [], 10, Expiry));
    }

    [Fact]
    public void Accept_SetsAcceptedStateAndAcceptingUser()
    {
        CampaignInvitation invite = Pending();

        invite.Accept(userId: 42);

        invite.Status.ShouldBe(InvitationStatus.Accepted);
        invite.AcceptedByUserId.ShouldBe(42);
        invite.RespondedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Decline_SetsDeclinedState()
    {
        CampaignInvitation invite = Pending();

        invite.Decline();

        invite.Status.ShouldBe(InvitationStatus.Declined);
        invite.AcceptedByUserId.ShouldBeNull();
        invite.RespondedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_SetsRevokedState()
    {
        CampaignInvitation invite = Pending();

        invite.Revoke();

        invite.Status.ShouldBe(InvitationStatus.Revoked);
        invite.RespondedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Accept_Throws_WhenNotPending()
    {
        CampaignInvitation invite = Pending();
        invite.Decline();

        Should.Throw<InvalidOperationException>(() => invite.Accept(1));
    }

    [Fact]
    public void Decline_Throws_WhenNotPending()
    {
        CampaignInvitation invite = Pending();
        invite.Revoke();

        Should.Throw<InvalidOperationException>(() => invite.Decline());
    }

    [Fact]
    public void Revoke_Throws_WhenNotPending()
    {
        CampaignInvitation invite = Pending();
        invite.Accept(1);

        Should.Throw<InvalidOperationException>(() => invite.Revoke());
    }
}
