namespace Worker.Application.Options;

public sealed class InvitationPurgeOptions
{
    public const string SectionName = "Invitations";

    public int TerminalRetentionDays { get; set; } = 30;
}
