using Ardalis.Result;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Invitations.RevokeInvitation;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class RevokeInvitationCommandHandlerTests
{
    private readonly ICampaignInvitationSecuredQueryRepo _queryRepo = Substitute.For<ICampaignInvitationSecuredQueryRepo>();
    private readonly ISecuredCommandRepo _commandRepo = Substitute.For<ISecuredCommandRepo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly DateTimeOffset Expiry = DateTimeOffset.UtcNow.AddDays(7);

    private RevokeInvitationCommandHandler Sut => new(_queryRepo, _commandRepo);

    private void Setup(params CampaignInvitation[] invitations)
    {
        _queryRepo.Invitations.Returns(invitations.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenInvitationNotVisible()
    {
        Setup();

        Result result = await Sut.Handle(new RevokeInvitationCommand(1, 5), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenInvitationNotPending()
    {
        Setup(TestCampaignInvitation.Create(5, campaignId: 1, "a@b.com", "h1", Expiry, InvitationStatus.Accepted));

        Result result = await Sut.Handle(new RevokeInvitationCommand(1, 5), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_RevokesAndPersists_WhenPending()
    {
        CampaignInvitation invitation = TestCampaignInvitation.Create(5, campaignId: 1, "a@b.com", "h1", Expiry);
        Setup(invitation);

        Result result = await Sut.Handle(new RevokeInvitationCommand(1, 5), Ct);

        result.IsSuccess.ShouldBeTrue();
        invitation.Status.ShouldBe(InvitationStatus.Revoked);
        await _commandRepo.Received(1).UpdateAsync(invitation, true, Ct);
    }
}
