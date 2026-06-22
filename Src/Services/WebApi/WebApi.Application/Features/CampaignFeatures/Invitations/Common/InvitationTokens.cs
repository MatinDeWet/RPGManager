using System.Security.Cryptography;
using System.Text;

namespace WebApi.Application.Features.CampaignFeatures.Invitations.Common;

/// <summary>
/// Generates and hashes the one-time invitation token. The raw token is returned to the issuing
/// Dungeon Master exactly once; only its SHA-256 hash is persisted, so a leaked database cannot be
/// used to accept invitations.
/// </summary>
internal static class InvitationTokens
{
    private const int TokenBytes = 32;

    public static (string Raw, string Hash) Generate()
    {
        byte[] raw = RandomNumberGenerator.GetBytes(TokenBytes);
        string token = Base64UrlEncode(raw);

        return (token, Hash(token));
    }

    public static string Hash(string rawToken)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
