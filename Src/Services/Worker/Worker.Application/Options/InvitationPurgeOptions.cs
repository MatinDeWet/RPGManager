namespace Worker.Application.Options;

public sealed class InvitationPurgeOptions
{
    public const string SectionName = "Invitations";

    /// <summary>
    /// How many days a terminal (accepted/declined/revoked) or expired invitation is retained for
    /// auditing before the purge job deletes it.
    /// </summary>
    public int TerminalRetentionDays { get; set; } = 30;
}
