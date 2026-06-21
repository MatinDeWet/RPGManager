namespace BlobStorage.Contracts.Models;

/// <summary>
/// Lightweight metadata for a stored blob, obtained without downloading its payload.
/// </summary>
/// <param name="Key">The blob key.</param>
/// <param name="ContentType">The stored content type, if known.</param>
/// <param name="ContentLength">The payload length in bytes.</param>
public sealed record BlobMetadata(string Key, string? ContentType, long ContentLength);
