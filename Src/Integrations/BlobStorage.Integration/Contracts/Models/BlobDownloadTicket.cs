namespace BlobStorage.Contracts.Models;

/// <summary>
/// A short-lived presigned GET URL for reading/downloading a blob.
/// </summary>
/// <param name="Key">The blob key the download URL is scoped to.</param>
/// <param name="DownloadUrl">The presigned GET URL.</param>
/// <param name="ExpiresAt">The instant the URL stops being valid.</param>
public sealed record BlobDownloadTicket(string Key, Uri DownloadUrl, DateTimeOffset ExpiresAt);
