using Ardalis.Result;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Members.KickCampaignMember;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Members;

public class KickCampaignMemberCommandHandlerTests
{
    private readonly ICampaignMemberSecuredQueryRepo _queryRepo = Substitute.For<ICampaignMemberSecuredQueryRepo>();
    private readonly ISecuredCommandRepo _commandRepo = Substitute.For<ISecuredCommandRepo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private KickCampaignMemberCommandHandler Sut => new(_queryRepo, _commandRepo);

    private void SetupMembers(params CampaignMember[] members)
    {
        _queryRepo.CampaignMembers.Returns(members.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenTargetIsNotVisible()
    {
        SetupMembers();

        Result result = await Sut.Handle(new KickCampaignMemberCommand(1, 20), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenTargetIsTheDungeonMaster()
    {
        SetupMembers(TestEntities.Member(campaignId: 1, userId: 10, CampaignRole.DungeonMaster));

        Result result = await Sut.Handle(new KickCampaignMemberCommand(1, 10), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_DeletesMember_WhenTargetIsAPlayer()
    {
        CampaignMember target = TestEntities.Member(campaignId: 1, userId: 20, CampaignRole.Player);
        SetupMembers(target);

        Result result = await Sut.Handle(new KickCampaignMemberCommand(1, 20), Ct);

        result.IsSuccess.ShouldBeTrue();
        await _commandRepo.Received(1).DeleteAsync(target, true, Ct);
    }

    [Fact]
    public async Task Handle_PropagatesUnauthorized_WhenLockDeniesTheDelete()
    {
        CampaignMember target = TestEntities.Member(campaignId: 1, userId: 20, CampaignRole.Player);
        SetupMembers(target);
        _commandRepo.When(x => x.DeleteAsync(target, true, Ct)).Do(_ => throw new UnauthorizedAccessException());

        await Should.ThrowAsync<UnauthorizedAccessException>(() => Sut.Handle(new KickCampaignMemberCommand(1, 20), Ct));
    }
}
