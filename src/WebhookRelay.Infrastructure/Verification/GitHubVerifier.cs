using System.Security.Cryptography;
using System.Text;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Verification;

public class GitHubVerifier : ISignatureVerifier
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "GitHub", StringComparison.OrdinalIgnoreCase);

    public bool Verify(string payload, IReadOnlyDictionary<string, string> headers, string secret)
    {
        if (!headers.TryGetValue("x-hub-signature-256", out var sig))
            return false;

        if (!sig.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            return false;

        var hexSig = sig["sha256=".Length..];
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);
        var expected = HMACSHA256.HashData(keyBytes, messageBytes);
        var expectedHex = Convert.ToHexString(expected).ToLowerInvariant();

        var actualBytes = Convert.FromHexString(hexSig.ToLowerInvariant());
        var expectedBytes = Convert.FromHexString(expectedHex);
        return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    public string? ExtractEventId(string payload, IReadOnlyDictionary<string, string> headers)
    {
        headers.TryGetValue("x-github-delivery", out var deliveryId);
        return deliveryId;
    }

    public string? ExtractEventType(string payload, IReadOnlyDictionary<string, string> headers)
    {
        headers.TryGetValue("x-github-event", out var eventType);
        return eventType;
    }
}
