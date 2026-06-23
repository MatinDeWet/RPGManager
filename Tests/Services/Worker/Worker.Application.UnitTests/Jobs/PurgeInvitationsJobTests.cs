using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Worker.Application.Jobs;
using Xunit;

namespace Worker.Application.UnitTests.Jobs;

public class PurgeInvitationsJobTests
{
    private const int RetentionDays = 30;

    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private static readonly DateTimeOffset Cutoff = Now.AddDays(-RetentionDays);

    private static Func<CampaignInvitation, bool> Purgeable => PurgeInvitationsJob.IsPurgeable(Cutoff).Compile();

    private static CampaignInvitation Invitation(
        InvitationStatus status,
        DateTimeOffset expiresAt,
        DateTimeOffset? respondedAt = null,
        DateTimeOffset? created = null)
    {
        var invitation = CampaignInvitation.Create(campaignId: 1, "invitee@example.com", tokenHash: "h", issuedByUserId: 1, expiresAt);

        switch (status)
        {
            case InvitationStatus.Accepted:
                invitation.Accept(userId: 2);
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

        if (created.HasValue)
        {
            SetProperty(invitation, nameof(CampaignInvitation.DateCreated), created.Value);
        }

        if (respondedAt.HasValue)
        {
            SetProperty(invitation, nameof(CampaignInvitation.RespondedAt), respondedAt.Value);
        }

        return invitation;
    }

    private static void SetProperty(CampaignInvitation invitation, string name, object value)
    {
        typeof(CampaignInvitation).GetProperty(name)!.SetValue(invitation, value);
    }

    [Fact]
    public void IsPurgeable_IsTrue_ForTerminalInvitationResolvedBeforeCutoff()
    {
        Purgeable(Invitation(InvitationStatus.Accepted, expiresAt: Now.AddDays(-33), respondedAt: Now.AddDays(-31))).ShouldBeTrue();
    }

    [Fact]
    public void IsPurgeable_IsTrue_ForInvitationThatExpiredWhilePendingBeforeCutoff()
    {
        Purgeable(Invitation(InvitationStatus.Pending, expiresAt: Now.AddDays(-31))).ShouldBeTrue();
    }

    [Fact]
    public void IsPurgeable_IsFalse_ForActivePendingInvitation()
    {
        Purgeable(Invitation(InvitationStatus.Pending, expiresAt: Now.AddDays(6))).ShouldBeFalse();
    }

    [Fact]
    public void IsPurgeable_IsFalse_ForRecentlyResolvedTerminalInvitation()
    {
        Purgeable(Invitation(InvitationStatus.Revoked, expiresAt: Now.AddDays(-1), respondedAt: Now.AddDays(-5))).ShouldBeFalse();
    }

    [Fact]
    public void IsPurgeable_IsFalse_ForOldInvitationResolvedRecently()
    {
        CampaignInvitation invitation = Invitation(
            InvitationStatus.Accepted,
            expiresAt: Now.AddDays(-33),
            respondedAt: Now.AddDays(-1),
            created: Now.AddDays(-40));

        Purgeable(invitation).ShouldBeFalse();
    }
}
