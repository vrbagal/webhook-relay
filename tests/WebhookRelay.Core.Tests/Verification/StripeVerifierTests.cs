using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using WebhookRelay.Infrastructure.Verification;
using Xunit;

namespace WebhookRelay.Core.Tests.Verification;

public class StripeVerifierTests
{
    private readonly StripeVerifier _verifier = new();

    [Fact]
    public void CanHandle_Stripe_ReturnsTrue() =>
        _verifier.CanHandle("Stripe").Should().BeTrue();

    [Fact]
    public void CanHandle_GitHub_ReturnsFalse() =>
        _verifier.CanHandle("GitHub").Should().BeFalse();

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        const string secret = "whsec_test_secret";
        const string payload = """{"type":"charge.succeeded","id":"evt_123"}""";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signedPayload = $"{timestamp}.{payload}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(signedPayload);
        var signature = HMACSHA256.HashData(keyBytes, messageBytes);
        var sigHex = Convert.ToHexString(signature).ToLowerInvariant();

        var headers = new Dictionary<string, string>
        {
            ["stripe-signature"] = $"t={timestamp},v1={sigHex}",
        };

        _verifier.Verify(payload, headers, secret).Should().BeTrue();
    }

    [Fact]
    public void Verify_InvalidSignature_ReturnsFalse()
    {
        var headers = new Dictionary<string, string>
        {
            ["stripe-signature"] = "t=1234,v1=invalidsig",
        };

        _verifier.Verify("payload", headers, "secret").Should().BeFalse();
    }

    [Fact]
    public void Verify_MissingHeader_ReturnsFalse()
    {
        var headers = new Dictionary<string, string>();
        _verifier.Verify("payload", headers, "secret").Should().BeFalse();
    }

    [Fact]
    public void ExtractEventType_ParsesJsonType()
    {
        const string payload = """{"type":"payment_intent.created"}""";
        var eventType = _verifier.ExtractEventType(payload, new Dictionary<string, string>());
        eventType.Should().Be("payment_intent.created");
    }
}
