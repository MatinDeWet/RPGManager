using Shared.Domain.Entities;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="Session"/> instances with a specific identifier. <see cref="Session.Id"/> has a
/// protected setter (it is database-generated), so it is assigned via reflection for tests that need to
/// exercise membership-based filtering.
/// </summary>
internal static class TestSession
{
    public static Session WithId(long id, long campaignId = 1)
    {
        var session = Session.Create(campaignId, sessionNumber: 1, $"session-{id}", DateTimeOffset.UtcNow, summary: null);

        typeof(Session).GetProperty(nameof(Session.Id))!.SetValue(session, id);

        return session;
    }
}
