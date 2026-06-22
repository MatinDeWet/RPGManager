namespace WebApi.Application.Common.Options;

public sealed class InvitationOptions
{
    public const string SectionName = "Invitations";

    /// <summary>How long a newly created invitation remains valid before it expires.</summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(7);
}
