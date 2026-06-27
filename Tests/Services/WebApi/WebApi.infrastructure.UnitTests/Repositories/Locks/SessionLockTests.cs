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

public class SessionLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private void SetupMembers(params CampaignMember[] members)
    {
        DbSet<CampaignMember> set = members.ToList().BuildMockDbSet();
        _context.Set<CampaignMember>().Returns(set);
    }

    private void SetupSessions(params Session[] sessions)
    {
        DbSet<Session> set = sessions.ToList().BuildMockDbSet();
        _context.Set<Session>().Returns(set);
    }

    [Fact]
    public async Task Secured_ReturnsOnlySessionsOfCampaignsTheUserIsAMemberOf()
    {
        SetupSessions(
            TestSession.WithId(1, campaignId: 1),
            TestSession.WithId(2, campaignId: 2),
            TestSession.WithId(3, campaignId: 3));
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.Player),
            TestCampaignMember.For(campaignId: 2, userId: 20, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 3, userId: 10, CampaignRole.DungeonMaster));

        SessionLock sut = new(_context);

        List<Session> result = await sut.Secured(10).ToListAsync(Ct);

        result.Select(x => x.Id).ShouldBe([1, 3], ignoreOrder: true);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_WhenTheUserIsNotAMemberOfAnyCampaign()
    {
        SetupSessions(TestSession.WithId(1, campaignId: 1), TestSession.WithId(2, campaignId: 2));
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 2, userId: 20, CampaignRole.Player));

        SessionLock sut = new(_context);

        List<Session> result = await sut.Secured(99).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_ForDungeonMaster_OnMutatingOperations(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        SessionLock sut = new(_context);
        Session session = TestSession.WithId(1, campaignId: 1);

        bool result = await sut.HasAccess(session, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_ForPlayer_OnMutatingOperations(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        SessionLock sut = new(_context);
        Session session = TestSession.WithId(1, campaignId: 1);

        bool result = await sut.HasAccess(session, userId: 5, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasAccess_IsTrue_ForAnyMember_OnRead()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        SessionLock sut = new(_context);
        Session session = TestSession.WithId(1, campaignId: 1);

        bool result = await sut.HasAccess(session, userId: 5, RepositoryOperationEnum.Read, Ct);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasAccess_IsFalse_ForNonMember()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        SessionLock sut = new(_context);
        Session session = TestSession.WithId(1, campaignId: 1);

        bool result = await sut.HasAccess(session, userId: 99, RepositoryOperationEnum.Update, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_IsTrue_ForSession()
    {
        SessionLock sut = new(_context);

        sut.IsMatch(typeof(Session)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_IsFalse_ForUnrelatedType()
    {
        SessionLock sut = new(_context);

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
