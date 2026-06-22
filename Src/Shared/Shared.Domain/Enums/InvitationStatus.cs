namespace Shared.Domain.Enums;

/// <summary>
/// The lifecycle state of a <see cref="Entities.CampaignInvitation"/>. There is no stored
/// <c>Expired</c> state — expiry is always derived from <see cref="Entities.CampaignInvitation.ExpiresAt"/>.
/// </summary>
public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Revoked = 4,
}
