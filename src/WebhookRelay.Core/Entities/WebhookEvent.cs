namespace WebhookRelay.Core.Entities;

public class WebhookEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EndpointId { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public string HeadersJson { get; set; } = "{}";
    public string? ProviderEventId { get; set; }
    public string? EventType { get; set; }
    public bool SignatureVerified { get; set; }
    public bool IsDuplicate { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public WebhookEndpoint Endpoint { get; set; } = null!;
    public ICollection<DeliveryAttempt> DeliveryAttempts { get; set; } = new List<DeliveryAttempt>();
}
