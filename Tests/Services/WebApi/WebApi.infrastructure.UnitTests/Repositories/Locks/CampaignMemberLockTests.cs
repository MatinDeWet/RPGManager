using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using Repository.Enums;
using Shouldly;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Data.Contexts;
using WebApi.infrastructure.Repositories.Locks;
using WebApi.infrastructure.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.infrastructure.UnitTests.Repositories.Locks;

public class CampaignMemberLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private void SetupMembers(params CampaignMember[] members)
    {
        DbSet<CampaignMember> set = members.ToList().BuildMockDbSet();
        _context.Set<CampaignMember>().Returns(set);
    }

    [Fact]
    public async Task Secured_ReturnsAllMembersOfCampaignsTheUserBelongsTo()
    {
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 1, userId: 20, CampaignRole.Player),
            TestCampaignMember.For(campaignId: 2, userId: 30, CampaignRole.DungeonMaster));

        CampaignMemberLock sut = new(_context);

        List<CampaignMember> result = await sut.Secured(10).ToListAsync(Ct);

        result.Select(x => x.UserId).ShouldBe([10, 20], ignoreOrder: true);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_WhenUserBelongsToNoCampaign()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.Player));

        CampaignMemberLock sut = new(_context);

        List<CampaignMember> result = await sut.Secured(99).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_ForDungeonMaster(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignMemberLock sut = new(_context);
        CampaignMember target = TestCampaignMember.For(campaignId: 1, userId: 20, CampaignRole.Player);

        bool result = await sut.HasAccess(target, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_ForPlayer(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        CampaignMemberLock sut = new(_context);
        CampaignMember target = TestCampaignMember.For(campaignId: 1, userId: 20, CampaignRole.Player);

        bool result = await sut.HasAccess(target, userId: 5, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasAccess_IsFalse_ForNonMember()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignMemberLock sut = new(_context);
        CampaignMember target = TestCampaignMember.For(campaignId: 1, userId: 20, CampaignRole.Player);

        bool result = await sut.HasAccess(target, userId: 99, RepositoryOperationEnum.Delete, Ct);

        result.ShouldBeFalse();
    }
}
