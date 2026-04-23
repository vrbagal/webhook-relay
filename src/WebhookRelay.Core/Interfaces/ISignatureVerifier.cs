namespace WebhookRelay.Core.Interfaces;

public interface ISignatureVerifier
{
    bool CanHandle(string provider);
    bool Verify(string payload, IReadOnlyDictionary<string, string> headers, string secret);
    string? ExtractEventId(string payload, IReadOnlyDictionary<string, string> headers);
    string? ExtractEventType(string payload, IReadOnlyDictionary<string, string> headers);
}
