namespace BlobStorage.Contracts.Models;

/// <summary>
/// A streamed blob returned from a direct download. The caller owns and MUST dispose
/// <see cref="Content"/>.
/// </summary>
/// <param name="Key">The blob key.</param>
/// <param name="Content">The blob payload stream. The caller is responsible for disposing it.</param>
/// <param name="ContentType">The stored content type, if known.</param>
/// <param name="ContentLength">The payload length in bytes.</param>
public sealed record BlobContent(string Key, Stream Content, string? ContentType, long ContentLength);
