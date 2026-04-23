using WebhookRelay.Core.Enums;

namespace WebhookRelay.Core.Entities;

public class WebhookEndpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ProviderType Provider { get; set; }
    public string SigningSecret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool RejectUnverified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WebhookEvent> Events { get; set; } = new List<WebhookEvent>();
    public ICollection<DeliveryTarget> Targets { get; set; } = new List<DeliveryTarget>();
}
