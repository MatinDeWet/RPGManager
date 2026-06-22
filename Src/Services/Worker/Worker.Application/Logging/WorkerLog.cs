using Microsoft.Extensions.Logging;

namespace Worker.Application.Logging;

internal static partial class WorkerLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "ExampleJob ran. There are {UserCount} user(s) in the database.")]
    public static partial void ExampleJobRan(this ILogger logger, int userCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "PurgeInvitationsJob ran. Deleted {Count} terminal or expired invitation(s).")]
    public static partial void PurgedInvitations(this ILogger logger, int count);
}
