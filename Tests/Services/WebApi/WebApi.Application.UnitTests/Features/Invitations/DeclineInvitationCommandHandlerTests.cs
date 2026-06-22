using Ardalis.Result;
using Identification.Constants;
using Identification.Contracts;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using WebApi.Application.Features.CampaignFeatures.Invitations.DeclineInvitation;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class DeclineInvitationCommandHandlerTests
{
    private const string RawToken = "raw-token";
    private const string Email = "invitee@example.com";

    private readonly ICampaignInvitationUnsecuredQueryRepo _queryRepo = Substitute.For<ICampaignInvitationUnsecuredQueryRepo>();
    private readonly IUnsecuredCommandRepo _commandRepo = Substitute.For<IUnsecuredCommandRepo>();
    private readonly IIdentityInfo _identityInfo = Substitute.For<IIdentityInfo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DeclineInvitationCommandHandler Sut => new(_queryRepo, _commandRepo, _identityInfo);

    public DeclineInvitationCommandHandlerTests()
    {
        _identityInfo.GetValue(ClaimConstants.Email).Returns(Email);
    }

    private static string TokenHash => InvitationTokens.Hash(RawToken);

    private void SetupInvitation(CampaignInvitation invitation)
    {
        _queryRepo.Invitations.Returns(new[] { invitation }.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTokenDoesNotMatch()
    {
        _queryRepo.Invitations.Returns(Array.Empty<CampaignInvitation>().BuildMock());

        Result result = await Sut.Handle(new DeclineInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsForbidden_WhenEmailDoesNotMatch()
    {
        _identityInfo.GetValue(ClaimConstants.Email).Returns("someone-else@example.com");
        SetupInvitation(TestEntities.Invitation(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7)));

        Result result = await Sut.Handle(new DeclineInvitationCommand(RawToken), Ct);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_DeclinesAndPersists_OnSuccess()
    {
        CampaignInvitation invitation = TestEntities.Invitation(1, 1, Email, TokenHash, DateTimeOffset.UtcNow.AddDays(7));
        SetupInvitation(invitation);

        Result result = await Sut.Handle(new DeclineInvitationCommand(RawToken), Ct);

        result.IsSuccess.ShouldBeTrue();
        invitation.Status.ShouldBe(InvitationStatus.Declined);
        await _commandRepo.Received(1).UpdateAsync(invitation, true, Ct);
    }
}
