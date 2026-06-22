using Ardalis.Result;
using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
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
    private readonly ICampaignSecuredQueryRepo _queryRepo = Substitute.For<ICampaignSecuredQueryRepo>();
    private readonly ISecuredCommandRepo _commandRepo = Substitute.For<ISecuredCommandRepo>();
    private readonly IIdentityInfo _identityInfo = Substitute.For<IIdentityInfo>();
    private readonly IOptions<InvitationOptions> _options = Options.Create(new InvitationOptions { Lifetime = TimeSpan.FromDays(7) });

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private CreateInvitationCommandHandler Sut => new(_queryRepo, _commandRepo, _identityInfo, _options);

    public CreateInvitationCommandHandlerTests()
    {
        _identityInfo.GetInternalUserId().Returns(10L);
    }

    private void SetupVisible(bool visible)
    {
        Campaign[] campaigns = visible ? [TestEntities.Campaign(id: 1, dungeonMasterUserId: 10)] : [];
        _queryRepo.Campaigns.Returns(campaigns.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCampaignNotVisible()
    {
        SetupVisible(false);

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, "a@b.com"), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsTokenAndPersists_OnSuccess()
    {
        SetupVisible(true);

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, "a@b.com"), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Token.ShouldNotBeNullOrWhiteSpace();
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
        await _commandRepo.Received(1).InsertAsync(Arg.Any<CampaignInvitation>(), true, Ct);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenPendingInvitationAlreadyExists()
    {
        SetupVisible(true);
        _commandRepo.When(x => x.InsertAsync(Arg.Any<CampaignInvitation>(), true, Ct))
            .Do(_ => throw new DbUpdateException("duplicate"));

        Result<CreateInvitationResponse> result = await Sut.Handle(new CreateInvitationCommand(1, "a@b.com"), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }
}
