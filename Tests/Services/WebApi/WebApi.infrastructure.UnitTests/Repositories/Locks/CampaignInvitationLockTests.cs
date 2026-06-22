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

public class CampaignInvitationLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private void SetupMembers(params CampaignMember[] members)
    {
        DbSet<CampaignMember> set = members.ToList().BuildMockDbSet();
        _context.Set<CampaignMember>().Returns(set);
    }

    private void SetupInvitations(params CampaignInvitation[] invitations)
    {
        DbSet<CampaignInvitation> set = invitations.ToList().BuildMockDbSet();
        _context.Set<CampaignInvitation>().Returns(set);
    }

    [Fact]
    public async Task Secured_ReturnsOnlyInvitationsOfCampaignsWhereTheUserIsDungeonMaster()
    {
        SetupInvitations(
            TestCampaignInvitation.For(id: 1, campaignId: 1),
            TestCampaignInvitation.For(id: 2, campaignId: 2),
            TestCampaignInvitation.For(id: 3, campaignId: 3));
        SetupMembers(
            TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.DungeonMaster),
            TestCampaignMember.For(campaignId: 2, userId: 10, CampaignRole.Player),
            TestCampaignMember.For(campaignId: 3, userId: 20, CampaignRole.DungeonMaster));

        CampaignInvitationLock sut = new(_context);

        List<CampaignInvitation> result = await sut.Secured(10).ToListAsync(Ct);

        result.Select(x => x.Id).ShouldBe([1], ignoreOrder: true);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_ForNonDungeonMaster()
    {
        SetupInvitations(TestCampaignInvitation.For(id: 1, campaignId: 1));
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 10, CampaignRole.Player));

        CampaignInvitationLock sut = new(_context);

        List<CampaignInvitation> result = await sut.Secured(10).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_ForDungeonMaster(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignInvitationLock sut = new(_context);
        CampaignInvitation invitation = TestCampaignInvitation.For(id: 1, campaignId: 1);

        bool result = await sut.HasAccess(invitation, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_ForPlayer(RepositoryOperationEnum operation)
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.Player));
        CampaignInvitationLock sut = new(_context);
        CampaignInvitation invitation = TestCampaignInvitation.For(id: 1, campaignId: 1);

        bool result = await sut.HasAccess(invitation, userId: 5, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasAccess_IsFalse_ForNonMember()
    {
        SetupMembers(TestCampaignMember.For(campaignId: 1, userId: 5, CampaignRole.DungeonMaster));
        CampaignInvitationLock sut = new(_context);
        CampaignInvitation invitation = TestCampaignInvitation.For(id: 1, campaignId: 1);

        bool result = await sut.HasAccess(invitation, userId: 99, RepositoryOperationEnum.Update, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_IsTrue_ForCampaignInvitation()
    {
        CampaignInvitationLock sut = new(_context);

        sut.IsMatch(typeof(CampaignInvitation)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_IsFalse_ForUnrelatedType()
    {
        CampaignInvitationLock sut = new(_context);

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
