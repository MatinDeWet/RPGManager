using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="World"/> instances owned by a given user. <see cref="World.Id"/> has a
/// protected setter (it is database-generated), so it is assigned via reflection for tests that
/// need to exercise ownership-based filtering.
/// </summary>
internal static class TestWorld
{
    public static World Owned(long id, long userId)
    {
        var world = World.Create(userId, $"world-{id}", description: null);

        typeof(World).GetProperty(nameof(World.Id))!.SetValue(world, id);

        return world;
    }
}
