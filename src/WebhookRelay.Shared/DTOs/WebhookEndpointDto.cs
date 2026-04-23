namespace WebhookRelay.Shared.DTOs;

public record WebhookEndpointDto(
    Guid Id,
    string Name,
    string Provider,
    bool IsActive,
    bool RejectUnverified,
    string IngestUrl,
    DateTime CreatedAt,
    IReadOnlyList<DeliveryTargetDto> Targets);

public record CreateEndpointRequest(
    string Name,
    string Provider,
    string SigningSecret,
    bool RejectUnverified = false);

public record UpdateEndpointRequest(
    string Name,
    bool IsActive,
    bool RejectUnverified,
    string? SigningSecret);
