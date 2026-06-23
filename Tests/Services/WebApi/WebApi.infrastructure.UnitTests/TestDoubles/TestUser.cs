using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

internal static class TestUser
{
    public static User WithId(long id)
    {
        var user = User.Create($"external-{id}", $"user-{id}@example.com");

        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);

        return user;
    }
}
