using Microsoft.Extensions.Logging;

namespace BlobStorage.Logging;

internal static partial class BlobStorageLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Created blob container '{Container}'.")]
    public static partial void ContainerCreated(this ILogger logger, string container);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Blob container '{Container}' already existed (created concurrently).")]
    public static partial void ContainerCreatedConcurrently(this ILogger logger, string container);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create blob container '{Container}'.")]
    public static partial void ContainerCreateFailed(this ILogger logger, Exception exception, string container);
}
