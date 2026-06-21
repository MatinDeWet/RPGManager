namespace BlobStorage.Contracts.Models;

/// <summary>
/// The result of reserving a blob key: a short-lived presigned PUT URL the client uploads to.
/// </summary>
/// <param name="Key">The blob key the upload URL is scoped to.</param>
/// <param name="UploadUrl">The presigned PUT URL.</param>
/// <param name="ExpiresAt">The instant the URL stops being valid.</param>
public sealed record BlobUploadTicket(string Key, Uri UploadUrl, DateTimeOffset ExpiresAt);
