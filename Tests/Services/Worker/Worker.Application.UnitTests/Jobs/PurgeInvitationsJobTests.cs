using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Worker.Application.Jobs;
using Worker.Application.Options;
using Worker.Application.Repositories.CommandRepos.UnsecuredRepos;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;
using Xunit;

namespace Worker.Application.UnitTests.Jobs;

public class PurgeInvitationsJobTests
{
    private const int RetentionDays = 30;

    private readonly ICampaignInvitationUnsecuredQueryRepo _queryRepo = Substitute.For<ICampaignInvitationUnsecuredQueryRepo>();
    private readonly IUnsecuredCommandRepo _commandRepo = Substitute.For<IUnsecuredCommandRepo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private PurgeInvitationsJob Sut => new(
        _queryRepo,
        _commandRepo,
        Microsoft.Extensions.Options.Options.Create(new InvitationPurgeOptions { TerminalRetentionDays = RetentionDays }),
        NullLogger<PurgeInvitationsJob>.Instance);

    private static CampaignInvitation Invitation(string tokenHash, DateTimeOffset created, DateTimeOffset expiresAt, InvitationStatus status)
    {
        var invitation = CampaignInvitation.Create(campaignId: 1, "invitee@example.com", tokenHash, issuedByUserId: 1, expiresAt);

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

        typeof(CampaignInvitation).GetProperty(nameof(CampaignInvitation.DateCreated))!.SetValue(invitation, created);

        return invitation;
    }

    private void SetupInvitations(params CampaignInvitation[] invitations)
    {
        _queryRepo.Invitations.Returns(invitations.BuildMock());
    }

    [Fact]
    public async Task RunAsync_DeletesTerminalOrExpiredInvitationsOlderThanRetention()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        CampaignInvitation terminalOld = Invitation("a", created: now.AddDays(-40), expiresAt: now.AddDays(-33), InvitationStatus.Accepted);
        CampaignInvitation expiredOld = Invitation("b", created: now.AddDays(-40), expiresAt: now.AddDays(-33), InvitationStatus.Pending);
        CampaignInvitation pendingActive = Invitation("c", created: now.AddDays(-1), expiresAt: now.AddDays(6), InvitationStatus.Pending);
        CampaignInvitation terminalRecent = Invitation("d", created: now.AddDays(-5), expiresAt: now.AddDays(2), InvitationStatus.Revoked);

        SetupInvitations(terminalOld, expiredOld, pendingActive, terminalRecent);

        await Sut.RunAsync(Ct);

        await _commandRepo.Received(1).DeleteAsync(terminalOld, Ct);
        await _commandRepo.Received(1).DeleteAsync(expiredOld, Ct);
        await _commandRepo.DidNotReceive().DeleteAsync(pendingActive, Ct);
        await _commandRepo.DidNotReceive().DeleteAsync(terminalRecent, Ct);
        await _commandRepo.Received(1).SaveAsync(Ct);
    }

    [Fact]
    public async Task RunAsync_DoesNothing_WhenNoStaleInvitations()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        SetupInvitations(Invitation("c", created: now.AddDays(-1), expiresAt: now.AddDays(6), InvitationStatus.Pending));

        await Sut.RunAsync(Ct);

        await _commandRepo.DidNotReceive().DeleteAsync(Arg.Any<CampaignInvitation>(), Ct);
        await _commandRepo.DidNotReceive().SaveAsync(Ct);
    }
}
