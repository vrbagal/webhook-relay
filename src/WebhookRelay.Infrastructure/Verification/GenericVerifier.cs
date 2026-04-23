using System.Security.Cryptography;
using System.Text;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Verification;

public class GenericVerifier : ISignatureVerifier
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "Generic", StringComparison.OrdinalIgnoreCase);

    public bool Verify(string payload, IReadOnlyDictionary<string, string> headers, string secret)
    {
        var sigHeader = headers.GetValueOrDefault("x-webhook-signature")
                     ?? headers.GetValueOrDefault("x-signature");

        if (sigHeader is null)
            return false;

        var cleanSig = sigHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? sigHeader["sha256=".Length..]
            : sigHeader;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);
        var expected = HMACSHA256.HashData(keyBytes, messageBytes);
        var expectedHex = Convert.ToHexString(expected).ToLowerInvariant();

        try
        {
            var actualBytes = Convert.FromHexString(cleanSig.ToLowerInvariant());
            var expectedBytes = Convert.FromHexString(expectedHex);
            return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
        }
        catch
        {
            return false;
        }
    }

    public string? ExtractEventId(string payload, IReadOnlyDictionary<string, string> headers) =>
        headers.GetValueOrDefault("x-webhook-id");

    public string? ExtractEventType(string payload, IReadOnlyDictionary<string, string> headers) =>
        headers.GetValueOrDefault("x-webhook-event");
}
