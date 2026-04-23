using WebhookRelay.Core.Enums;

namespace WebhookRelay.Core.Entities;

public class DeliveryAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Guid TargetId { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsReplay { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public int? HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextRetryAt { get; set; }

    public WebhookEvent Event { get; set; } = null!;
    public DeliveryTarget Target { get; set; } = null!;
}
