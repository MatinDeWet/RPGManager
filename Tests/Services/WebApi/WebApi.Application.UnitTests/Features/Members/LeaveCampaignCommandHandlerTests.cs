using Ardalis.Result;
using Identification.Contracts;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Members.LeaveCampaign;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Members;

public class LeaveCampaignCommandHandlerTests
{
    private readonly ICampaignMemberUnsecuredQueryRepo _queryRepo = Substitute.For<ICampaignMemberUnsecuredQueryRepo>();
    private readonly IUnsecuredCommandRepo _commandRepo = Substitute.For<IUnsecuredCommandRepo>();
    private readonly IIdentityInfo _identityInfo = Substitute.For<IIdentityInfo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private LeaveCampaignCommandHandler Sut => new(_queryRepo, _commandRepo, _identityInfo);

    public LeaveCampaignCommandHandlerTests()
    {
        _identityInfo.GetInternalUserId().Returns(20L);
    }

    private void SetupMembers(params CampaignMember[] members)
    {
        _queryRepo.CampaignMembers.Returns(members.BuildMock());
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCallerIsNotAMember()
    {
        SetupMembers();

        Result result = await Sut.Handle(new LeaveCampaignCommand(1), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenCallerIsTheDungeonMaster()
    {
        SetupMembers(TestCampaignMember.Create(campaignId: 1, userId: 20, CampaignRole.DungeonMaster));

        Result result = await Sut.Handle(new LeaveCampaignCommand(1), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Handle_DeletesMembership_WhenCallerIsAPlayer()
    {
        CampaignMember membership = TestCampaignMember.Create(campaignId: 1, userId: 20, CampaignRole.Player);
        SetupMembers(membership);

        Result result = await Sut.Handle(new LeaveCampaignCommand(1), Ct);

        result.IsSuccess.ShouldBeTrue();
        await _commandRepo.Received(1).DeleteAsync(membership, true, Ct);
    }
}
