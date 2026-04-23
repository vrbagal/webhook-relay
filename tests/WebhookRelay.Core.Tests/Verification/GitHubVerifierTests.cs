using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using WebhookRelay.Infrastructure.Verification;
using Xunit;

namespace WebhookRelay.Core.Tests.Verification;

public class GitHubVerifierTests
{
    private readonly GitHubVerifier _verifier = new();

    [Fact]
    public void CanHandle_GitHub_ReturnsTrue() =>
        _verifier.CanHandle("GitHub").Should().BeTrue();

    [Fact]
    public void Verify_ValidSignature_ReturnsTrue()
    {
        const string secret = "github_secret";
        const string payload = """{"action":"opened"}""";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(keyBytes, messageBytes);
        var sigHex = Convert.ToHexString(hash).ToLowerInvariant();

        var headers = new Dictionary<string, string>
        {
            ["x-hub-signature-256"] = $"sha256={sigHex}",
            ["x-github-event"] = "pull_request",
            ["x-github-delivery"] = "abc-123",
        };

        _verifier.Verify(payload, headers, secret).Should().BeTrue();
    }

    [Fact]
    public void ExtractEventType_ReturnsHeader()
    {
        var headers = new Dictionary<string, string>
        {
            ["x-github-event"] = "push",
        };
        _verifier.ExtractEventType("payload", headers).Should().Be("push");
    }

    [Fact]
    public void ExtractEventId_ReturnsDeliveryId()
    {
        var headers = new Dictionary<string, string>
        {
            ["x-github-delivery"] = "delivery-uuid-123",
        };
        _verifier.ExtractEventId("payload", headers).Should().Be("delivery-uuid-123");
    }
}
