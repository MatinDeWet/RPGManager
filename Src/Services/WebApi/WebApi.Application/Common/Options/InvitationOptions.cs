namespace WebApi.Application.Common.Options;

public sealed class InvitationOptions
{
    public const string SectionName = "Invitations";

    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(7);
}
