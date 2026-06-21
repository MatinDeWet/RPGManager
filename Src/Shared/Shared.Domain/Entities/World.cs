using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;

namespace Shared.Domain.Entities;

public class World : Entity<long>
{
    public long UserId { get; private set; }

    public virtual User User { get; private set; } = null!;

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public static World Create(long userId, string name, string? description)
    {
        return new World
        {
            UserId = userId,
            Name = ValidName(name),
            Description = ValidDescription(description),
        };
    }

    public void Update(string name, string? description)
    {
        Name = ValidName(name);
        Description = ValidDescription(description);
    }

    private static string ValidName(string name)
    {
        return Guard.Against.ValidString(name, nameof(name), maxLength: 128);
    }

    private static string? ValidDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        return Guard.Against.ValidString(description, nameof(description), maxLength: 4096);
    }
}
