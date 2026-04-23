using System.Security.Cryptography;
using System.Text;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Verification;

public class TwilioVerifier : ISignatureVerifier
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "Twilio", StringComparison.OrdinalIgnoreCase);

    public bool Verify(string payload, IReadOnlyDictionary<string, string> headers, string secret)
    {
        if (!headers.TryGetValue("x-twilio-signature", out var sig))
            return false;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);
        var expected = HMACSHA1.HashData(keyBytes, messageBytes);
        var expectedB64 = Convert.ToBase64String(expected);

        var actualBytes = Convert.FromBase64String(sig);
        var expectedBytes = Convert.FromBase64String(expectedB64);
        return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    public string? ExtractEventId(string payload, IReadOnlyDictionary<string, string> headers) => null;

    public string? ExtractEventType(string payload, IReadOnlyDictionary<string, string> headers)
    {
        try
        {
            var pairs = payload.Split('&');
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && Uri.UnescapeDataString(kv[0]) == "EventType")
                    return Uri.UnescapeDataString(kv[1]);
            }
        }
        catch { }
        return null;
    }
}
