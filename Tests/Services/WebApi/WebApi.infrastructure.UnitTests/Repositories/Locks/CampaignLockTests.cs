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

public class CampaignLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private void SetupMembers(params CampaignMember[] members)
    {
        DbSet<CampaignMember> set = members.ToList().BuildMockDbSet();
        _context.Set<CampaignMember>().Returns(set);
    }

    private void SetupCampaigns(params Campaign[] campaigns)
    {
        DbSet<Campaign> set = campaigns.ToList().BuildMockDbSet();
        _context.Set<Campaign>().Returns(set);
    }

    [Fact]
    public async Task Secured_ReturnsOnlyCampaignsTheUserIsAMemberOf()
    {
        SetupCampaigns(TestCampaign.WithId(1), TestCampaign.WithId(2), TestCampaign.WithId(3));
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.Player),
            TestCampaignMember.For(campaignId: 2, userId: 20, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 3, userId: 10, CampaignRole.DungeonMaster));

        CampaignLock sut = new(_context);

        List<Campaign> result = await sut.Secured(10).ToListAsync(Ct);

        result.Select(x => x.Id).ShouldBe([1, 3], ignoreOrder: true);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_WhenTheUserIsNotAMemberOfAnyCampaign()
    {
        SetupCampaigns(TestCampaign.WithId(1), TestCampaign.WithId(2));
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 2, userId: 20, CampaignRole.Player));

        CampaignLock sut = new(_context);

        List<Campaign> result = await sut.Secured(99).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task HasAccess_AllowsInsert_RegardlessOfMembership()
    {
        SetupMembers();
        CampaignLock sut = new(_context);
        Campaign campaign = TestCampaign.WithId(1);

        bool result = await sut.HasAccess(campaign, userId: 5, RepositoryOperationEnum.Insert, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_ForDungeonMaster_OnMutatingOperations(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignLock sut = new(_context);
        Campaign campaign = TestCampaign.WithId(1);

        bool result = await sut.HasAccess(campaign, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_ForPlayer_OnMutatingOperations(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        CampaignLock sut = new(_context);
        Campaign campaign = TestCampaign.WithId(1);

        bool result = await sut.HasAccess(campaign, userId: 5, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasAccess_IsTrue_ForAnyMember_OnRead()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        CampaignLock sut = new(_context);
        Campaign campaign = TestCampaign.WithId(1);

        bool result = await sut.HasAccess(campaign, userId: 5, RepositoryOperationEnum.Read, Ct);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasAccess_IsFalse_ForNonMember()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignLock sut = new(_context);
        Campaign campaign = TestCampaign.WithId(1);

        bool result = await sut.HasAccess(campaign, userId: 99, RepositoryOperationEnum.Update, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_IsTrue_ForCampaign()
    {
        CampaignLock sut = new(_context);

        sut.IsMatch(typeof(Campaign)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_IsFalse_ForUnrelatedType()
    {
        CampaignLock sut = new(_context);

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
