using Ardalis.Result;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Common.Options;
using WebApi.Application.Features.CampaignFeatures.Invitations.CreateInvitation;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class CreateInvitationCommandHandlerTests
{
    private const string Email = "a@b.com";

    private readonly ICampaignSecuredQueryRepo _campaignQueryRepo = Substitute.For<ICampaignSecuredQueryRepo>();
    private readonly ICampaignInvitationSecuredQueryRepo _invitationQueryRepo = Substitute.For<ICampaignInvitationSecuredQueryRepo>();
    private readonly ISecuredCommandRepo _commandRepo = Substitute.For<ISecuredCommandRepo>();
    private readonly IIdentityInfo _identityInfo = Substitute.For<IIdentityInfo>();
    private readonly IOptions<InvitationOptions> _options = Options.Create(new InvitationOptions { Lifetime = TimeSpan.FromDays(7) });

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private CreateInvitationCommandHandler Sut => new(_campaignQueryRepo, _invitationQueryRepo, _commandRepo, _identityInfo, _options);

    public CreateInvitationCommandHandlerTests()
    {
        _identityInfo.GetInternalUserId().Returns(10L);
        _invitationQueryRepo.Invitations.Returns(Array.Empty<CampaignInvitation>().BuildMock());
    }

    private void SetupVisible(bool visible)
    {
        Campaign[] campaigns = visible ? [TestEntities.Campaign(id: 1, dungeonMasterUserId: 10)] : [];
        _campaignQueryRepo.Campaigns.Returns(campaigns.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCampaignNotVisible()
    {
        SetupVisible(false);

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, Email), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsTokenAndPersists_OnSuccess()
    {
        SetupVisible(true);

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, Email), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Token.ShouldNotBeNullOrWhiteSpace();
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        await _commandRepo.Received(1).InsertAsync(Arg.Any<CampaignInvitation>(), Ct);
        await _commandRepo.Received(1).SaveAsync(Ct);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenValidPendingInvitationExists()
    {
        SetupVisible(true);
        CampaignInvitation existing = TestEntities.Invitation(2, campaignId: 1, Email, tokenHash: "h", DateTimeOffset.UtcNow.AddDays(3));
        _invitationQueryRepo.Invitations.Returns(new[] { existing }.BuildMock());

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, Email), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
        await _commandRepo.DidNotReceive().InsertAsync(Arg.Any<CampaignInvitation>(), Ct);
    }

    [Fact]
    public async Task Handle_RevokesStaleInvitationAndIssuesNew_WhenExistingPendingHasExpired()
    {
        SetupVisible(true);
        CampaignInvitation expired = TestEntities.Invitation(2, campaignId: 1, Email, tokenHash: "h", DateTimeOffset.UtcNow.AddMinutes(-1));
        _invitationQueryRepo.Invitations.Returns(new[] { expired }.BuildMock());

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, Email), Ct);

        result.IsSuccess.ShouldBeTrue();
        expired.Status.ShouldBe(InvitationStatus.Revoked);
        await _commandRepo.Received(1).UpdateAsync(expired, Ct);
        await _commandRepo.Received(1).InsertAsync(Arg.Any<CampaignInvitation>(), Ct);
        await _commandRepo.Received(1).SaveAsync(Ct);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenConcurrentInsertViolatesUniqueIndex()
    {
        SetupVisible(true);
        CampaignInvitation racingPending = TestEntities.Invitation(3, campaignId: 1, Email, tokenHash: "h", DateTimeOffset.UtcNow.AddDays(3));

        // Empty on the pre-check (so we attempt the insert), then a conflicting pending invitation on
        // the post-failure re-check, modelling a concurrent create that won the race.
        _invitationQueryRepo.Invitations.Returns(
            Array.Empty<CampaignInvitation>().BuildMock(),
            new[] { racingPending }.BuildMock());
        _commandRepo.When(x => x.SaveAsync(Ct)).Do(_ => throw new DbUpdateException("duplicate"));

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, Email), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }
}
