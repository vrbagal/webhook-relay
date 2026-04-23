namespace WebhookRelay.Shared.DTOs;

public record WebhookEventDto(
    Guid Id,
    Guid EndpointId,
    string EndpointName,
    string RawPayload,
    Dictionary<string, string> Headers,
    string? ProviderEventId,
    string? EventType,
    bool SignatureVerified,
    bool IsDuplicate,
    DateTime ReceivedAt,
    IReadOnlyList<DeliveryAttemptDto> DeliveryAttempts);

public record ReplayRequest(
    string? OverrideTargetUrl = null,
    bool StripOriginalHeaders = false);
