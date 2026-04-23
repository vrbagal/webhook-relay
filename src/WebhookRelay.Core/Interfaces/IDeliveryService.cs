using WebhookRelay.Core.Entities;

namespace WebhookRelay.Core.Interfaces;

public interface IDeliveryService
{
    Task DeliverAsync(WebhookEvent webhookEvent, DeliveryTarget target,
        int attemptNumber, bool isReplay, string? overrideUrl = null,
        CancellationToken ct = default);
}
