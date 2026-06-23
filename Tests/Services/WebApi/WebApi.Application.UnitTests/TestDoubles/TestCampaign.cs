using Shared.Domain.Entities;

namespace WebApi.Application.UnitTests.TestDoubles;

internal static class TestCampaign
{
    public static Campaign Create(long id, long dungeonMasterUserId)
    {
        var campaign = Campaign.Create(worldId: 1, creatorUserId: dungeonMasterUserId, $"campaign-{id}", description: null);

        TestEntity.SetId(campaign, id);

        return campaign;
    }
}
