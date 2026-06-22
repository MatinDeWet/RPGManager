using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;
using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

public class Campaign : Entity<long>
{
    public long WorldId { get; private set; }

    public virtual World World { get; private set; } = null!;

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public virtual ICollection<CampaignMember> Members { get; private set; } = [];

    public virtual ICollection<CampaignInvitation> Invitations { get; private set; } = [];

    public static Campaign Create(long worldId, long creatorUserId, string name, string? description)
    {
        var campaign = new Campaign
        {
            WorldId = worldId,
            Name = ValidName(name),
            Description = ValidDescription(description),
        };

        // The creator is always enrolled as the campaign's first Dungeon Master.
        campaign.Members.Add(CampaignMember.Create(creatorUserId, CampaignRole.DungeonMaster));

        return campaign;
    }

    public void Update(string name, string? description)
    {
        Name = ValidName(name);
        Description = ValidDescription(description);
    }

    public void AddPlayer(long userId)
    {
        if (Members.Any(member => member.UserId == userId))
        {
            throw new InvalidOperationException($"User '{userId}' is already a member of this campaign.");
        }

        Members.Add(CampaignMember.Create(userId, CampaignRole.Player));
    }

    public void RemoveMember(long userId)
    {
        CampaignMember member = Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException($"User '{userId}' is not a member of this campaign.");

        if (member.Role == CampaignRole.DungeonMaster)
        {
            throw new InvalidOperationException("The Dungeon Master cannot be removed from the campaign.");
        }

        Members.Remove(member);
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
