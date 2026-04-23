namespace WebhookRelay.Shared.DTOs;

public record DeliveryAttemptDto(
    Guid Id,
    Guid EventId,
    Guid TargetId,
    string TargetName,
    string TargetUrl,
    int AttemptNumber,
    bool IsReplay,
    string Status,
    int? HttpStatusCode,
    string? ResponseBody,
    string? ErrorMessage,
    long DurationMs,
    DateTime AttemptedAt,
    DateTime? NextRetryAt);
