namespace WebhookRelay.Shared.DTOs;

public record DeliveryTargetDto(
    Guid Id,
    Guid EndpointId,
    string Name,
    string TargetUrl,
    bool IsActive,
    int TimeoutSeconds,
    IReadOnlyList<RoutingRuleDto> RoutingRules);

public record CreateTargetRequest(
    string Name,
    string TargetUrl,
    int TimeoutSeconds = 30);

public record RoutingRuleDto(
    Guid Id,
    Guid TargetId,
    string JsonPath,
    string Operator,
    string? Value);

/// <summary>
/// Operators: equals | not_equals | contains | not_contains | exists | not_exists | starts_with | ends_with
/// Value is ignored for exists / not_exists.
/// </summary>
public record CreateRoutingRuleRequest(
    string JsonPath,
    string Operator,
    string? Value);
