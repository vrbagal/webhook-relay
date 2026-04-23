using System.Security.Cryptography;
using System.Text;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Verification;

public class StripeVerifier : ISignatureVerifier
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "Stripe", StringComparison.OrdinalIgnoreCase);

    public bool Verify(string payload, IReadOnlyDictionary<string, string> headers, string secret)
    {
        if (!headers.TryGetValue("stripe-signature", out var sigHeader))
            return false;

        var parts = sigHeader.Split(',');
        string? timestamp = null;
        string? v1 = null;

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0] == "t") timestamp = kv[1];
            else if (kv[0] == "v1") v1 = kv[1];
        }

        if (timestamp is null || v1 is null)
            return false;

        var signedPayload = $"{timestamp}.{payload}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(signedPayload);
        var expected = HMACSHA256.HashData(keyBytes, messageBytes);
        var expectedHex = Convert.ToHexString(expected).ToLowerInvariant();

        try
        {
            var actualBytes = Convert.FromHexString(v1);
            var expectedBytes = Convert.FromHexString(expectedHex);
            return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
        }
        catch
        {
            return false;
        }
    }

    public string? ExtractEventId(string payload, IReadOnlyDictionary<string, string> headers) => null;

    public string? ExtractEventType(string payload, IReadOnlyDictionary<string, string> headers)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("type", out var t))
                return t.GetString();
        }
        catch { }
        return null;
    }
}
