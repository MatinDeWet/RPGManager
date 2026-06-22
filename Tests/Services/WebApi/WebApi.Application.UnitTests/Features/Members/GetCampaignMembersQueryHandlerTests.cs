using Ardalis.Result;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Members.GetCampaignMembers;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Members;

public class GetCampaignMembersQueryHandlerTests
{
    private readonly ICampaignSecuredQueryRepo _queryRepo = Substitute.For<ICampaignSecuredQueryRepo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private GetCampaignMembersQueryHandler Sut => new(_queryRepo);

    [Fact]
    public async Task Handle_ReturnsMembers_WhenCampaignIsVisible()
    {
        Campaign campaign = TestEntities.Campaign(id: 1, dungeonMasterUserId: 10);
        campaign.Members.Add(CampaignMember.Create(userId: 20, CampaignRole.Player));
        _queryRepo.Campaigns.Returns(new[] { campaign }.BuildMock());

        Result<IReadOnlyList<GetCampaignMembersResponse>> result = await Sut.Handle(new GetCampaignMembersQuery(1), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.UserId).ShouldBe([10, 20], ignoreOrder: true);
        result.Value.ShouldContain(x => x.UserId == 10 && x.Role == CampaignRole.DungeonMaster);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenCampaignIsNotVisible()
    {
        _queryRepo.Campaigns.Returns(Array.Empty<Campaign>().BuildMock());

        Result<IReadOnlyList<GetCampaignMembersResponse>> result = await Sut.Handle(new GetCampaignMembersQuery(99), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
