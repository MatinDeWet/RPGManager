using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Xunit;

namespace Shared.Domain.UnitTests.Entities;

public class CampaignTests
{
    private const long Dm = 1;

    private static Campaign NewCampaign()
    {
        return Campaign.Create(worldId: 1, creatorUserId: Dm, "campaign", description: null);
    }

    [Fact]
    public void Create_EnrolsCreatorAsDungeonMaster()
    {
        Campaign campaign = NewCampaign();

        CampaignMember member = campaign.Members.ShouldHaveSingleItem();
        member.UserId.ShouldBe(Dm);
        member.Role.ShouldBe(CampaignRole.DungeonMaster);
    }

    [Fact]
    public void AddPlayer_AddsMemberAsPlayer()
    {
        Campaign campaign = NewCampaign();

        campaign.AddPlayer(userId: 2);

        CampaignMember player = campaign.Members.Single(m => m.UserId == 2);
        player.Role.ShouldBe(CampaignRole.Player);
    }

    [Fact]
    public void AddPlayer_Throws_WhenAlreadyAMember()
    {
        Campaign campaign = NewCampaign();
        campaign.AddPlayer(userId: 2);

        Should.Throw<InvalidOperationException>(() => campaign.AddPlayer(userId: 2));
    }

    [Fact]
    public void AddPlayer_Throws_WhenUserIsTheDungeonMaster()
    {
        Campaign campaign = NewCampaign();

        Should.Throw<InvalidOperationException>(() => campaign.AddPlayer(userId: Dm));
    }

    [Fact]
    public void RemoveMember_RemovesAPlayer()
    {
        Campaign campaign = NewCampaign();
        campaign.AddPlayer(userId: 2);

        campaign.RemoveMember(userId: 2);

        campaign.Members.ShouldNotContain(m => m.UserId == 2);
    }

    [Fact]
    public void RemoveMember_Throws_WhenUserIsNotAMember()
    {
        Campaign campaign = NewCampaign();

        Should.Throw<InvalidOperationException>(() => campaign.RemoveMember(userId: 99));
    }

    [Fact]
    public void RemoveMember_Throws_WhenTargetingTheDungeonMaster()
    {
        Campaign campaign = NewCampaign();

        Should.Throw<InvalidOperationException>(() => campaign.RemoveMember(userId: Dm));
    }
}
