using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="Campaign"/> instances with a specific identifier. <see cref="Campaign.Id"/> has
/// a protected setter (it is database-generated), so it is assigned via reflection for tests that need
/// to exercise membership-based filtering.
/// </summary>
internal static class TestCampaign
{
    public static Campaign WithId(long id, long worldId = 1)
    {
        var campaign = Campaign.Create(worldId, creatorUserId: 0, $"campaign-{id}", description: null);

        typeof(Campaign).GetProperty(nameof(Campaign.Id))!.SetValue(campaign, id);

        return campaign;
    }
}
