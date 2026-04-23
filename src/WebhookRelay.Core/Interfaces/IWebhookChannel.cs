using WebhookRelay.Core.Entities;

namespace WebhookRelay.Core.Interfaces;

public interface IWebhookChannel
{
    void Enqueue(WebhookEvent webhookEvent);
    IAsyncEnumerable<WebhookEvent> ReadAllAsync(CancellationToken ct);
}
