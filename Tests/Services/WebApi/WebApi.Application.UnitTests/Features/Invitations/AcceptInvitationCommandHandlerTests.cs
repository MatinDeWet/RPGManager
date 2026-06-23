using Ardalis.Result;
using Identification.Constants;
using Identification.Contracts;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Invitations.AcceptInvitation;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class AcceptInvitationCommandHandlerTests
{
    private const string RawToken = "raw-token";
    private const string Email = "invitee@example.com";
    private const long UserId = 20;

    private readonly ICampaignInvitationUnsecuredQueryRepo _invitationQueryRepo = Substitute.For<ICampaignInvitationUnsecuredQueryRepo>();
    private readonly ICampaignMemberUnsecuredQueryRepo _memberQueryRepo = Substitute.For<ICampaignMemberUnsecuredQueryRepo>();
    private readonly IUnsecuredCommandRepo _commandRepo = Substitute.For<IUnsecuredCommandRepo>();
    private readonly IIdentityInfo _identityInfo = Substitute.For<IIdentityInfo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AcceptInvitationCommandHandler Sut => new(_invitationQueryRepo, _memberQueryRepo, _commandRepo, _identityInfo);

    public AcceptInvitationCommandHandlerTests()
    {
        _identityInfo.GetInternalUserId().Returns(UserId);
        _identityInfo.GetValue(ClaimConstants.Email).Returns(Email);
        _identityInfo.GetValue(ClaimConstants.EmailVerified).Returns("true");
        _memberQueryRepo.CampaignMembers.Returns(Array.Empty<CampaignMember>().BuildMock());
    }

    private static string TokenHash => InvitationTokens.Hash(RawToken);

    private void SetupInvitation(CampaignInvitation invitation)
    {
        _invitationQueryRepo.Invitations.Returns(new[] { invitation }.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTokenDoesNotMatch()
    {
        _invitationQueryRepo.Invitations.Returns(Array.Empty<CampaignInvitation>().BuildMock());

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenInvitationNotPending()
    {
        SetupInvitation(TestCampaignInvitation.Create(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7), InvitationStatus.Revoked));

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenExpired()
    {
        SetupInvitation(TestCampaignInvitation.Create(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddMinutes(-1)));

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenEmailDoesNotMatch()
    {
        _identityInfo.GetValue(ClaimConstants.Email).Returns("someone-else@example.com");
        SetupInvitation(TestCampaignInvitation.Create(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7)));

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("")]
    public async Task Handle_ReturnsForbidden_WhenEmailNotVerified(string emailVerified)
    {
        _identityInfo.GetValue(ClaimConstants.EmailVerified).Returns(emailVerified);
        SetupInvitation(TestCampaignInvitation.Create(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7)));

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_AddsMemberAndAccepts_OnSuccess()
    {
        CampaignInvitation invitation = TestCampaignInvitation.Create(1, campaignId: 5, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7));
        SetupInvitation(invitation);

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.IsSuccess.ShouldBeTrue();
        invitation.Status.ShouldBe(InvitationStatus.Accepted);
        invitation.AcceptedByUserId.ShouldBe(UserId);
        await _commandRepo.Received(1).UpdateAsync(invitation, Ct);
        await _commandRepo.Received(1).InsertAsync(
            Arg.Is<CampaignMember>(m => m.CampaignId == 5 && m.UserId == UserId && m.Role == CampaignRole.Player), Ct);
        await _commandRepo.Received(1).SaveAsync(Ct);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenAlreadyAMember()
    {
        CampaignInvitation invitation = TestCampaignInvitation.Create(1, campaignId: 5, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7));
        SetupInvitation(invitation);
        _memberQueryRepo.CampaignMembers.Returns(new[] { TestCampaignMember.Create(campaignId: 5, userId: UserId, CampaignRole.Player) }.BuildMock());

        Result result = await Sut.Handle(new AcceptInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
        invitation.Status.ShouldBe(InvitationStatus.Pending);
        await _commandRepo.DidNotReceive().InsertAsync(Arg.Any<CampaignMember>(), Ct);
        await _commandRepo.DidNotReceive().SaveAsync(Ct);
    }
}
