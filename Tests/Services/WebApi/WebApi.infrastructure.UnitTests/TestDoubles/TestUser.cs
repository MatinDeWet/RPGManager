using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="User"/> instances with a specific identifier. <see cref="User.Id"/> has a
/// protected setter (it is database-generated), so it is assigned via reflection for tests that
/// need to exercise identity-based filtering.
/// </summary>
internal static class TestUser
{
    public static User WithId(long id)
    {
        var user = User.Create($"external-{id}");

        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);

        return user;
    }
}
